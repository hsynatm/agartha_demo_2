using AMMS.Core.Interfaces;
using AssetManagement.Domain.Entities;

namespace AssetManagement.Domain.Repositories;

public interface IAssetManagementRepository : IRepository<Asset>
{
    Task<Asset?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
}
