namespace AMMS.Core.Interfaces;

public interface IUserAuthorizationService
{
    Task<Guid?> ResolveAppUserIdAsync(string keycloakUserId, CancellationToken cancellationToken = default);

    Task<bool> IsAuthorizedAsync(
        string keycloakUserId,
        IReadOnlyCollection<string> requiredRoles,
        IReadOnlyCollection<string> requiredRoleGroups,
        CancellationToken cancellationToken = default);
}
