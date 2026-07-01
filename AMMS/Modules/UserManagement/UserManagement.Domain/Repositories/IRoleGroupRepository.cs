using AMMS.Core.Interfaces;
using UserManagement.Domain.Entities;

namespace UserManagement.Domain.Repositories;

public interface IRoleGroupRepository : IRepository<RoleGroup>
{
    Task<RoleGroup?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<RoleGroup?> GetByIdWithRolesAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> CodeExistsAsync(string code, Guid? excludeRoleGroupId = null, CancellationToken cancellationToken = default);

    Task ReplaceRoleAssignmentsAsync(Guid roleGroupId,IReadOnlyCollection<Guid> roleIds,CancellationToken cancellationToken = default);
}
