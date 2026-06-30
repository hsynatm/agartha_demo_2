using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserManagement.Application.Services;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Persistence;

namespace UserManagement.Infrastructure.Keycloak;

public sealed class KeycloakUserBootstrapRunner
{
    private static readonly TimeSpan[] RetryDelays =
    [
        TimeSpan.Zero,
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(10),
        TimeSpan.FromSeconds(20),
        TimeSpan.FromSeconds(30)
    ];

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly KeycloakBootstrapOptions _options;
    private readonly ILogger<KeycloakUserBootstrapRunner> _logger;

    public KeycloakUserBootstrapRunner(
        IServiceScopeFactory scopeFactory,
        IOptions<KeycloakBootstrapOptions> options,
        ILogger<KeycloakUserBootstrapRunner> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("KeycloakBootstrap is disabled; DB users will not be synced to Keycloak.");
            return;
        }

        _logger.LogInformation(
            "KeycloakBootstrap started. Reconciling UserManagement.Users with Keycloak (password = username).");

        foreach (var delay in RetryDelays)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            if (delay > TimeSpan.Zero)
            {
                _logger.LogInformation(
                    "Retrying Keycloak user bootstrap in {DelaySeconds}s.",
                    delay.TotalSeconds);
                await Task.Delay(delay, cancellationToken);
            }

            if (await TryReconcileUsersAsync(cancellationToken))
            {
                return;
            }
        }

        _logger.LogError(
            "Keycloak user bootstrap failed after {AttemptCount} attempts. " +
            "Ensure Keycloak is running, then restart AMMS.Api.",
            RetryDelays.Length);
    }

    private async Task<bool> TryReconcileUsersAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUserManagementUnitOfWork>();
        var keycloakSync = scope.ServiceProvider.GetRequiredService<IKeycloakUserSyncService>();

        IReadOnlyList<AppUser> dbUsers;
        try
        {
            dbUsers = await unitOfWork.Users.GetActiveUsersForKeycloakBootstrapAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not read UserManagement users for Keycloak bootstrap.");
            return false;
        }

        if (dbUsers.Count == 0)
        {
            _logger.LogWarning("UserManagement.Users has no active users; nothing to sync to Keycloak.");
            return true;
        }

        var allSucceeded = true;
        var linkedCount = 0;
        var createdCount = 0;
        var repairedCount = 0;

        foreach (var user in dbUsers)
        {
            try
            {
                var keycloakPassword = KeycloakUserPassword.FromUsername(user.Username);
                var keycloakUserId = await keycloakSync.FindUserIdByUsernameAsync(user.Username, cancellationToken);

                if (keycloakUserId is not null)
                {
                    var previousId = user.KeycloakUserId;
                    user.KeycloakUserId = keycloakUserId;

                    if (_options.ResetPasswordOnReconcile)
                    {
                        await keycloakSync.UpdateUserAsync(user, keycloakPassword, cancellationToken);
                        _logger.LogInformation(
                            "Reset Keycloak password for {Username} to username (dev).",
                            user.Username);
                    }

                    if (!string.Equals(previousId, keycloakUserId, StringComparison.Ordinal))
                    {
                        _logger.LogWarning(
                            "Repaired KeycloakUserId for {Username}: DB={DbId}, Keycloak={KeycloakId}",
                            user.Username,
                            previousId ?? "(null)",
                            keycloakUserId);

                        unitOfWork.Users.Update(user);
                        await unitOfWork.SaveChangesAsync(cancellationToken);
                        repairedCount++;
                    }

                    linkedCount++;
                    continue;
                }

                user.KeycloakUserId = await keycloakSync.CreateUserAsync(
                    user,
                    keycloakPassword,
                    cancellationToken);

                unitOfWork.Users.Update(user);
                await unitOfWork.SaveChangesAsync(cancellationToken);
                createdCount++;

                _logger.LogInformation(
                    "Created Keycloak account for {Username}. Keycloak password = username.",
                    user.Username);
            }
            catch (Exception ex)
            {
                allSucceeded = false;
                _logger.LogError(ex, "Keycloak reconcile failed for user {Username}.", user.Username);
            }
        }

        _logger.LogInformation(
            "Keycloak reconcile finished. DB users={Total}, linked={Linked}, created={Created}, repaired={Repaired}.",
            dbUsers.Count,
            linkedCount,
            createdCount,
            repairedCount);

        return allSucceeded;
    }
}
