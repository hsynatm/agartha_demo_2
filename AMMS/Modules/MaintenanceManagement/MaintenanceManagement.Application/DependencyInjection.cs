using MaintenanceManagement.Application.Mappings;
using MaintenanceManagement.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MaintenanceManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddMaintenanceManagementApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => cfg.AddProfile<WorkOrderMapping>());
        services.AddScoped<IMaintenanceManagementService, MaintenanceManagementService>();
        return services;
    }
}
