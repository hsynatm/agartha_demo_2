using AMMS.Infrastructure.Persistence;
using AMMS.Infrastructure.Services;
using AssetManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AssetManagement.Infrastructure;

public sealed class AssetManagementDbContextFactory : IDesignTimeDbContextFactory<AssetManagementDbContext>
{
    public AssetManagementDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AssetManagementDbContext>();
        optionsBuilder.UseNpgsql(DesignTimeDbConnection.Resolve());
        return new AssetManagementDbContext(optionsBuilder.Options, CurrentUserService.Empty);
    }
}
