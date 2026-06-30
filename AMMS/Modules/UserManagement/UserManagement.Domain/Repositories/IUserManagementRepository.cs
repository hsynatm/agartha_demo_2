using AMMS.Core.Interfaces;
using UserManagement.Domain.Entities;

namespace UserManagement.Domain.Repositories;

public interface IUserManagementRepository : IRepository<AppUser>
{
    Task<AppUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    Task<AppUser?> GetByUsernameIncludingDeletedAsync(string username, CancellationToken cancellationToken = default);

    Task HardDeleteAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<AppUser?> GetByKeycloakUserIdAsync(string keycloakUserId, CancellationToken cancellationToken = default);

    Task<AppUser?> ResolveActiveUserAsync(
        string? keycloakUserId,
        string? username,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AppUser>> GetUsersPendingKeycloakSyncAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AppUser>> GetActiveUsersForKeycloakBootstrapAsync(CancellationToken cancellationToken = default);

    Task<AppUser?> GetByIdWithAssignmentsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> UsernameExistsAsync(string username, Guid? excludeUserId = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetEffectiveRoleCodesAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetUserRoleGroupCodesAsync(Guid userId, CancellationToken cancellationToken = default);

    Task ReplaceRoleAssignmentsAsync(
        Guid userId,
        IReadOnlyCollection<Guid> roleIds,
        IReadOnlyCollection<Guid> roleGroupIds,
        CancellationToken cancellationToken = default);
}
