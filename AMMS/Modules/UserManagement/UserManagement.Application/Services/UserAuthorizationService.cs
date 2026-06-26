using AMMS.Core.Interfaces;
using UserManagement.Domain.Repositories;

namespace UserManagement.Application.Services;

public sealed class UserAuthorizationService : IUserAuthorizationService
{
    private readonly IUserManagementRepository _users;

    public UserAuthorizationService(IUserManagementRepository users)
    {
        _users = users;
    }

    public async Task<Guid?> ResolveAppUserIdAsync(string keycloakUserId, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByKeycloakUserIdAsync(keycloakUserId, cancellationToken);
        return user is { IsActive: true } ? user.Id : null;
    }

    public async Task<bool> IsAuthorizedAsync(
        string keycloakUserId,
        IReadOnlyCollection<string> requiredRoles,
        IReadOnlyCollection<string> requiredRoleGroups,
        CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByKeycloakUserIdAsync(keycloakUserId, cancellationToken);
        if (user is not { IsActive: true })
        {
            return false;
        }

        if (requiredRoleGroups.Count > 0)
        {
            var assignedGroups = await _users.GetUserRoleGroupCodesAsync(user.Id, cancellationToken);
            if (assignedGroups.Any(group => requiredRoleGroups.Contains(group, StringComparer.OrdinalIgnoreCase)))
            {
                return true;
            }
        }

        if (requiredRoles.Count > 0)
        {
            var effectiveRoles = await _users.GetEffectiveRoleCodesAsync(user.Id, cancellationToken);
            if (effectiveRoles.Any(role => requiredRoles.Contains(role, StringComparer.OrdinalIgnoreCase)))
            {
                return true;
            }
        }

        return false;
    }
}
