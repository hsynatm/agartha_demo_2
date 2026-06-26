using AMMS.Shared.Models;
using UserManagement.Application.Dtos;

namespace UserManagement.Application.Services;

public interface IRoleGroupService
{
    Task<RoleGroupDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResult<RoleGroupDto>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default);

    Task<RoleGroupDto> CreateAsync(CreateRoleGroupDto request, CancellationToken cancellationToken = default);

    Task<RoleGroupDto> UpdateAsync(Guid id, UpdateRoleGroupDto request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
