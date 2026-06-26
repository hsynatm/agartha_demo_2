using AMMS.Infrastructure.Persistence;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Repositories;
using UserManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace UserManagement.Infrastructure.Repository;

public class RoleRepository : EfRepository<Role>, IRoleRepository
{
    private readonly UserManagementDbContext _context;

    public RoleRepository(UserManagementDbContext context)
        : base(context)
    {
        _context = context;
    }

    public async Task<Role?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        await _context.Roles.AsNoTracking().FirstOrDefaultAsync(role => role.Code == code, cancellationToken);

    public async Task<bool> CodeExistsAsync(string code, Guid? excludeRoleId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Roles.AsNoTracking().Where(role => role.Code == code);
        if (excludeRoleId.HasValue)
        {
            query = query.Where(role => role.Id != excludeRoleId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }
}
