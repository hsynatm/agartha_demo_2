using AMMS.Core.Interfaces;
using UserManagement.Domain.Entities;

namespace UserManagement.Domain.Repositories;

public interface IRoleRepository : IRepository<Role>
{
    Task<Role?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<bool> CodeExistsAsync(string code, Guid? excludeRoleId = null, CancellationToken cancellationToken = default);
}
