using AMMS.Core.Interfaces;
using AMMS.Infrastructure.Persistence;
using AssetManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Infrastructure.Persistence;

public class AssetManagementDbContext : BaseDbContext
{
    public AssetManagementDbContext(DbContextOptions<AssetManagementDbContext> options, ICurrentUserService currentUserService)
        : base(options, currentUserService)
    {
    }

    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<AssetDocument> AssetDocuments => Set<AssetDocument>();
    public DbSet<AssetLifeLimit> AssetLifeLimits => Set<AssetLifeLimit>();
    public DbSet<AssetLocation> AssetLocations => Set<AssetLocation>();
    public DbSet<AssetLocationHistory> AssetLocationHistories => Set<AssetLocationHistory>();
    public DbSet<AssetPart> AssetParts => Set<AssetPart>();
    public DbSet<AssetStatusHistory> AssetStatusHistories => Set<AssetStatusHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(DependencyInjection.SchemaName);
        base.OnModelCreating(modelBuilder);
    }
}
