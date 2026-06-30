namespace AMMS.Core.Interfaces;

public interface IUserAuthorizationService
{
    Task<Guid?> ResolveAppUserIdAsync(
        string keycloakUserId,
        string? username = null,
        CancellationToken cancellationToken = default);

    Task<bool> IsAuthorizedAsync(
        string keycloakUserId,
        IReadOnlyCollection<string> requiredRoles,
        IReadOnlyCollection<string> requiredRoleGroups,
        string? username = null,
        CancellationToken cancellationToken = default);
}
