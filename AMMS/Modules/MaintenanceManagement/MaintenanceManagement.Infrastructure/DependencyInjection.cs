using AMMS.Infrastructure.Auditing;
using MaintenanceManagement.Domain.Persistence;
using MaintenanceManagement.Domain.Repositories;
using MaintenanceManagement.Infrastructure.Persistence;
using MaintenanceManagement.Infrastructure.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace MaintenanceManagement.Infrastructure;

public static class DependencyInjection
{
    public const string SchemaName = "MaintenanceManagement";
    public const string ModuleName = "MaintenanceManagement";

    public static IServiceCollection AddMaintenanceManagementInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddAmmsModuleDbContext<MaintenanceManagementDbContext>(connectionString, ModuleName);

        services.AddScoped<IMaintenanceManagementRepository, MaintenanceManagementRepository>();
        services.AddScoped<IMaintenanceManagementUnitOfWork, MaintenanceManagementUnitOfWork>();

        return services;
    }
}
