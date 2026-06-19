using AMMS.Infrastructure.Persistence;
using AssetManagement.Domain.Persistence;
using AssetManagement.Domain.Repositories;

namespace AssetManagement.Infrastructure.Persistence;

public sealed class AssetManagementUnitOfWork : EfUnitOfWork<AssetManagementDbContext>, IAssetManagementUnitOfWork
{
    public AssetManagementUnitOfWork(
        AssetManagementDbContext context,
        IAssetManagementRepository assets)
        : base(context)
    {
        Assets = assets;
    }

    public IAssetManagementRepository Assets { get; }
}
