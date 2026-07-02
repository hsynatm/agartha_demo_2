using AMMS.Core.Exceptions;
using AMMS.Core.Localization;
using UserManagement.Domain.Repositories;

namespace UserManagement.Application.Services;

internal static class RoleCodeResolver
{
    public static async Task<IReadOnlyCollection<Guid>> ResolveRoleIdsAsync(
        IRoleRepository roles,
        IReadOnlyCollection<string> roleCodes,
        CancellationToken cancellationToken)
    {
        var roleIds = new List<Guid>();
        foreach (var code in roleCodes.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var role = await roles.GetByCodeAsync(code, cancellationToken);
            if (role is null)
            {
                throw new AmmsException.Business(
                    LocalizationKeys.Modules.UserManagement.RoleNotFound,
                    messageArgs: new { Code = code },
                    errorCode: LocalizationKeys.Modules.UserManagement.ErrorCodes.RoleNotFound);
            }

            roleIds.Add(role.Id);
        }

        return roleIds;
    }

    public static async Task<IReadOnlyCollection<Guid>> ResolveRoleGroupIdsAsync(
        IRoleGroupRepository roleGroups,
        IReadOnlyCollection<string> roleGroupCodes,
        CancellationToken cancellationToken)
    {
        var roleGroupIds = new List<Guid>();
        foreach (var code in roleGroupCodes.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var roleGroup = await roleGroups.GetByCodeAsync(code, cancellationToken);
            if (roleGroup is null)
            {
                throw new AmmsException.Business(
                    LocalizationKeys.Modules.UserManagement.RoleGroupNotFound,
                    messageArgs: new { Code = code },
                    errorCode: LocalizationKeys.Modules.UserManagement.ErrorCodes.RoleGroupNotFound);
            }

            roleGroupIds.Add(roleGroup.Id);
        }

        return roleGroupIds;
    }
}
