using AMMS.Shared.Models;
using UserManagement.Application.Dtos;

namespace UserManagement.Application.Services;

public interface IRoleService
{
    Task<RoleDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResult<RoleDto>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default);

    Task<RoleDto> CreateAsync(CreateRoleDto request, CancellationToken cancellationToken = default);

    Task<RoleDto> UpdateAsync(Guid id, UpdateRoleDto request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
