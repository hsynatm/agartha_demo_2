using UserManagement.Domain.Entities;

namespace UserManagement.Application.Services;

/// <summary>
/// Keycloak identity sync only (account, password, profile). Roles are managed in PostgreSQL.
/// </summary>
public interface IKeycloakUserSyncService
{
    Task<string> CreateUserAsync(AppUser user, string password, CancellationToken cancellationToken = default);

    Task UpdateUserAsync(AppUser user, string? newPassword, CancellationToken cancellationToken = default);

    Task DeleteUserAsync(string keycloakUserId, CancellationToken cancellationToken = default);

    Task<bool> UserExistsAsync(string keycloakUserId, CancellationToken cancellationToken = default);

    Task<string?> FindUserIdByUsernameAsync(string username, CancellationToken cancellationToken = default);
}
