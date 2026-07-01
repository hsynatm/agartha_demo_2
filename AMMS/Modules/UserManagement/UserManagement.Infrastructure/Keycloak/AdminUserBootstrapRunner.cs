using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Persistence;

namespace UserManagement.Infrastructure.Keycloak;

/// <summary>
/// Ensures the default admin user exists in UserManagement.Users before Keycloak sync.
/// </summary>
public sealed class AdminUserBootstrapRunner
{
    private const string AdminRoleCode = "admin";
    private const string UserManagementRoleGroupCode = "UserManagement";

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly KeycloakBootstrapOptions _options;
    private readonly ILogger<AdminUserBootstrapRunner> _logger;

    public AdminUserBootstrapRunner(IServiceScopeFactory scopeFactory,IOptions<KeycloakBootstrapOptions> options,ILogger<AdminUserBootstrapRunner> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled || !_options.EnsureAdminUser)
        {
            return;
        }

        var username = _options.AdminUsername.Trim();
        if (string.IsNullOrWhiteSpace(username))
        {
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUserManagementUnitOfWork>();

        try
        {
            var existing = await unitOfWork.Users.GetByUsernameAsync(username, cancellationToken);
            if (existing is not null)
            {
                _logger.LogDebug("Bootstrap admin user '{Username}' already exists in UserManagement.", username);
                return;
            }

            var deleted = await unitOfWork.Users.GetByUsernameIncludingDeletedAsync(username, cancellationToken);
            if (deleted is not null)
            {
                await unitOfWork.Users.HardDeleteAsync(deleted.Id, cancellationToken);
            }

            var adminRole = await unitOfWork.Roles.GetByCodeAsync(AdminRoleCode, cancellationToken);
            var userManagementGroup = await unitOfWork.RoleGroups.GetByCodeAsync(UserManagementRoleGroupCode, cancellationToken);

            if (adminRole is null || userManagementGroup is null)
            {
                _logger.LogWarning(
                    "Cannot seed admin user: roles not found. Run UserManagement migrations first (admin role, UserManagement role group).");
                return;
            }

            var admin = new AppUser
            {
                Username = username,
                Email = $"{username}@amms.local",
                FirstName = "System",
                LastName = "Admin",
                OrganizationId = "1",
                OrganizationName = "Test Organization",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await unitOfWork.Users.AddAsync(admin, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            await unitOfWork.Users.ReplaceRoleAssignmentsAsync(
                admin.Id,
                [adminRole.Id],
                [userManagementGroup.Id],
                cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Created bootstrap admin user '{Username}' in UserManagement (Keycloak password will be username on sync).",
                username);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to ensure admin user '{Username}' in UserManagement.", username);
        }
    }
}
