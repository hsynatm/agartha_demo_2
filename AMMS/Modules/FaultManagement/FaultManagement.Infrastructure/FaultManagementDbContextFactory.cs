using AMMS.Infrastructure.Persistence;
using AMMS.Infrastructure.Services;
using FaultManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FaultManagement.Infrastructure;

public sealed class FaultManagementDbContextFactory : IDesignTimeDbContextFactory<FaultManagementDbContext>
{
    public FaultManagementDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FaultManagementDbContext>();
        optionsBuilder.UseNpgsql(DesignTimeDbConnection.Resolve());
        return new FaultManagementDbContext(optionsBuilder.Options, CurrentUserService.Empty);
    }
}
