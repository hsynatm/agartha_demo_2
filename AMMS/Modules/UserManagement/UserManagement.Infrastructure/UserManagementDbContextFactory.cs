using AMMS.Infrastructure.Persistence;
using AMMS.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using UserManagement.Infrastructure.Persistence;

namespace UserManagement.Infrastructure;

public sealed class UserManagementDbContextFactory : IDesignTimeDbContextFactory<UserManagementDbContext>
{
    public UserManagementDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<UserManagementDbContext>();
        optionsBuilder.UseNpgsql(DesignTimeDbConnection.Resolve());
        return new UserManagementDbContext(optionsBuilder.Options, CurrentUserService.Empty);
    }
}
