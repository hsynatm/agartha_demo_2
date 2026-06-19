using AMMS.Infrastructure.Persistence;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Repositories;
using AssetManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Infrastructure.Repository;

public class AssetManagementRepository : EfRepository<Asset>, IAssetManagementRepository
{
    private readonly AssetManagementDbContext _context;

    public AssetManagementRepository(AssetManagementDbContext context)
        : base(context)
    {
        _context = context;
    }

    public async Task<Asset?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Assets
            .AsNoTracking()
            .Include(asset => asset.CurrentLocation)
            .Include(asset => asset.Documents)
            .Include(asset => asset.LocationHistories)
            .ThenInclude(history => history.Location)
            .Include(asset => asset.StatusHistories)
            .Include(asset => asset.Parts)
            .Include(asset => asset.LifeLimits)
            .FirstOrDefaultAsync(asset => asset.Id == id, cancellationToken);
}
