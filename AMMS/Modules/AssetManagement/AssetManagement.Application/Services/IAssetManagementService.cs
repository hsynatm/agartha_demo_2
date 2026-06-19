using AMMS.Shared.Models;
using AssetManagement.Application.Dtos;

namespace AssetManagement.Application.Services;

public interface IAssetManagementService
{
    Task<AssetDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResult<AssetDto>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default);

    Task<AssetDto> CreateAsync(AssetDto request, CancellationToken cancellationToken = default);

    Task<AssetDto> UpdateAsync(Guid id, AssetDto request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
