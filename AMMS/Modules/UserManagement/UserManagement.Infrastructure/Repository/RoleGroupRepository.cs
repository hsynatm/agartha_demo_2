using AMMS.Infrastructure.Persistence;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Repositories;
using UserManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace UserManagement.Infrastructure.Repository;

public class RoleGroupRepository : EfRepository<RoleGroup>, IRoleGroupRepository
{
    private readonly UserManagementDbContext _context;

    public RoleGroupRepository(UserManagementDbContext context)
        : base(context)
    {
        _context = context;
    }

    public async Task<RoleGroup?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        await _context.RoleGroups.AsNoTracking().FirstOrDefaultAsync(roleGroup => roleGroup.Code == code, cancellationToken);

    public async Task<RoleGroup?> GetByIdWithRolesAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.RoleGroups
            .AsNoTracking()
            .Include(roleGroup => roleGroup.RoleGroupRoles)
            .ThenInclude(roleGroupRole => roleGroupRole.Role)
            .FirstOrDefaultAsync(roleGroup => roleGroup.Id == id, cancellationToken);

    public async Task<bool> CodeExistsAsync(string code, Guid? excludeRoleGroupId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.RoleGroups.AsNoTracking().Where(roleGroup => roleGroup.Code == code);
        if (excludeRoleGroupId.HasValue)
        {
            query = query.Where(roleGroup => roleGroup.Id != excludeRoleGroupId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task ReplaceRoleAssignmentsAsync(
        Guid roleGroupId,
        IReadOnlyCollection<Guid> roleIds,
        CancellationToken cancellationToken = default)
    {
        var existing = await _context.RoleGroupRoles
            .Where(roleGroupRole => roleGroupRole.RoleGroupId == roleGroupId)
            .ToListAsync(cancellationToken);
        _context.RoleGroupRoles.RemoveRange(existing);

        foreach (var roleId in roleIds.Distinct())
        {
            await _context.RoleGroupRoles.AddAsync(
                new RoleGroupRole { RoleGroupId = roleGroupId, RoleId = roleId },
                cancellationToken);
        }
    }
}
