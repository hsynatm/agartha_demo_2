using AMMS.Infrastructure.Persistence;
using AMMS.Infrastructure.Services;
using MaintenanceManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MaintenanceManagement.Infrastructure;

public sealed class MaintenanceManagementDbContextFactory : IDesignTimeDbContextFactory<MaintenanceManagementDbContext>
{
    public MaintenanceManagementDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MaintenanceManagementDbContext>();
        optionsBuilder.UseNpgsql(DesignTimeDbConnection.Resolve());
        return new MaintenanceManagementDbContext(optionsBuilder.Options, CurrentUserService.Empty);
    }
}
