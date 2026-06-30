using AMMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Repositories;
using UserManagement.Infrastructure.Persistence;

namespace UserManagement.Infrastructure.Repository;

public class UserManagementRepository : EfRepository<AppUser>, IUserManagementRepository
{
    private readonly UserManagementDbContext _context;

    public UserManagementRepository(UserManagementDbContext context)
        : base(context)
    {
        _context = context;
    }

    public async Task<AppUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default) =>
        await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Username == username, cancellationToken);

    public async Task<AppUser?> GetByUsernameIncludingDeletedAsync(
        string username,
        CancellationToken cancellationToken = default) =>
        await _context.Users
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Username == username, cancellationToken);

    public async Task HardDeleteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await _context.UserRoles
            .IgnoreQueryFilters()
            .Where(userRole => userRole.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        await _context.UserRoleGroups
            .IgnoreQueryFilters()
            .Where(userRoleGroup => userRoleGroup.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        await _context.Users
            .IgnoreQueryFilters()
            .Where(user => user.Id == userId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<AppUser?> GetByKeycloakUserIdAsync(string keycloakUserId, CancellationToken cancellationToken = default) =>
        await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.KeycloakUserId == keycloakUserId, cancellationToken);

    public async Task<AppUser?> ResolveActiveUserAsync(
        string? keycloakUserId,
        string? username,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(keycloakUserId))
        {
            var byKeycloakId = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    user => user.KeycloakUserId == keycloakUserId && user.IsActive,
                    cancellationToken);

            if (byKeycloakId is not null)
            {
                return byKeycloakId;
            }
        }

        if (string.IsNullOrWhiteSpace(username))
        {
            return null;
        }

        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Username == username && user.IsActive, cancellationToken);
    }

    public async Task<IReadOnlyList<AppUser>> GetUsersPendingKeycloakSyncAsync(CancellationToken cancellationToken = default) =>
        await _context.Users
            .Where(user => user.KeycloakUserId == null || user.KeycloakUserId == string.Empty)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<AppUser>> GetActiveUsersForKeycloakBootstrapAsync(CancellationToken cancellationToken = default) =>
        await _context.Users
            .Where(user => user.IsActive)
            .OrderBy(user => user.Username)
            .ToListAsync(cancellationToken);

    public async Task<AppUser?> GetByIdWithAssignmentsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Users
            .AsNoTracking()
            .Include(user => user.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .Include(user => user.UserRoleGroups)
            .ThenInclude(userRoleGroup => userRoleGroup.RoleGroup)
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

    public async Task<bool> UsernameExistsAsync(
        string username,
        Guid? excludeUserId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Users.AsNoTracking().Where(user => user.Username == username);

        if (excludeUserId.HasValue)
        {
            query = query.Where(user => user.Id != excludeUserId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetEffectiveRoleCodesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var directRoles = await _context.UserRoles
            .AsNoTracking()
            .Where(userRole => userRole.UserId == userId)
            .Select(userRole => userRole.Role.Code)
            .ToListAsync(cancellationToken);

        var groupRoles = await _context.UserRoleGroups
            .AsNoTracking()
            .Where(userRoleGroup => userRoleGroup.UserId == userId)
            .SelectMany(
                userRoleGroup => userRoleGroup.RoleGroup.RoleGroupRoles.Select(roleGroupRole => roleGroupRole.Role.Code))
            .ToListAsync(cancellationToken);

        return directRoles
            .Concat(groupRoles)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task<IReadOnlyList<string>> GetUserRoleGroupCodesAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await _context.UserRoleGroups
            .AsNoTracking()
            .Where(userRoleGroup => userRoleGroup.UserId == userId)
            .Select(userRoleGroup => userRoleGroup.RoleGroup.Code)
            .Distinct()
            .ToListAsync(cancellationToken);

    public async Task ReplaceRoleAssignmentsAsync(
        Guid userId,
        IReadOnlyCollection<Guid> roleIds,
        IReadOnlyCollection<Guid> roleGroupIds,
        CancellationToken cancellationToken = default)
    {
        var existingRoles = await _context.UserRoles
            .Where(userRole => userRole.UserId == userId)
            .ToListAsync(cancellationToken);
        _context.UserRoles.RemoveRange(existingRoles);

        var existingGroups = await _context.UserRoleGroups
            .Where(userRoleGroup => userRoleGroup.UserId == userId)
            .ToListAsync(cancellationToken);
        _context.UserRoleGroups.RemoveRange(existingGroups);

        foreach (var roleId in roleIds.Distinct())
        {
            await _context.UserRoles.AddAsync(new UserRole { UserId = userId, RoleId = roleId }, cancellationToken);
        }

        foreach (var roleGroupId in roleGroupIds.Distinct())
        {
            await _context.UserRoleGroups.AddAsync(
                new UserRoleGroup { UserId = userId, RoleGroupId = roleGroupId },
                cancellationToken);
        }
    }
}
