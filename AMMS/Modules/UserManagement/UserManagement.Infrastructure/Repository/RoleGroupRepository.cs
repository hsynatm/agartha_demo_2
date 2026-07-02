using AMMS.Infrastructure.Persistence;
using AMMS.Shared.Models;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Repositories;
using UserManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace UserManagement.Infrastructure.Repository;

public class RoleGroupRepository : EfRepository<RoleGroup>, IRoleGroupRepository
{
    private readonly UserManagementDbContext _context;

    public RoleGroupRepository(UserManagementDbContext context): base(context)
    {
        _context = context;
    }

    public async Task<RoleGroup?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        await _context.RoleGroups.AsNoTracking().FirstOrDefaultAsync(roleGroup => roleGroup.Code == code, cancellationToken);

    public async Task<RoleGroup?> GetByIdWithRolesAsync(Guid id, CancellationToken cancellationToken = default) =>
        await RoleGroupsWithRolesQuery()
            .FirstOrDefaultAsync(roleGroup => roleGroup.Id == id, cancellationToken);

    public async Task<PagedResult<RoleGroup>> GetPagedWithRolesAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var normalizedPage = PagingDefaults.NormalizePage(page);
        var normalizedPageSize = pageSize < 1 ? PagingDefaults.DefaultPageSize : pageSize;
        var query = RoleGroupsWithRolesQuery();
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(roleGroup => roleGroup.CreatedAt)
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<RoleGroup>
        {
            Items = items,
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            TotalCount = totalCount
        };
    }

    public async Task<bool> CodeExistsAsync(string code, Guid? excludeRoleGroupId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.RoleGroups.AsNoTracking().Where(roleGroup => roleGroup.Code == code);
        if (excludeRoleGroupId.HasValue)
        {
            query = query.Where(roleGroup => roleGroup.Id != excludeRoleGroupId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task ReplaceRoleAssignmentsAsync(Guid roleGroupId,IReadOnlyCollection<Guid> roleIds,CancellationToken cancellationToken = default)
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

    private IQueryable<RoleGroup> RoleGroupsWithRolesQuery() =>
        _context.RoleGroups
            .AsNoTracking()
            .Include(roleGroup => roleGroup.RoleGroupRoles)
            .ThenInclude(roleGroupRole => roleGroupRole.Role);
}
