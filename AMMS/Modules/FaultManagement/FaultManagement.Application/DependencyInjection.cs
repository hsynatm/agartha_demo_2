using FaultManagement.Application.Mappings;
using FaultManagement.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FaultManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddFaultManagementApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => cfg.AddProfile<FaultReportMapping>());
        services.AddScoped<IFaultManagementService, FaultManagementService>();
        return services;
    }
}
