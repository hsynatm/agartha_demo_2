using AMMS.Shared.Models;
using UserManagement.Application.Dtos;

namespace UserManagement.Application.Services;

public interface IUserManagementService
{
    Task<UserDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResult<UserDto>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default);

    Task<UserDto> CreateAsync(CreateUserDto request, CancellationToken cancellationToken = default);

    Task<UserDto> UpdateAsync(Guid id, UpdateUserDto request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
