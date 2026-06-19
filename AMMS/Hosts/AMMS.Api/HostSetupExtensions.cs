using AMMS.Infrastructure;
using AMMS.Infrastructure.Auditing;
using AMMS.Infrastructure.Logging;
using AssetManagement.Application;
using AssetManagement.Infrastructure;
using FaultManagement.Application;
using FaultManagement.Infrastructure;
using MaintenanceManagement.Application;
using MaintenanceManagement.Infrastructure;
using Scalar.AspNetCore;

namespace AMMS.Api;

internal static class HostSetupExtensions
{
    public static bool IsApiDocumentationEnabled(this IHostEnvironment environment) =>
        environment.IsDevelopment() || environment.IsEnvironment("Test");

    public static IServiceCollection AddAmmsModules(this IServiceCollection services, string connectionString)
    {
        services.AddAmmsInfrastructure();
        services.AddAmmsAuditing(connectionString);
        services.AddAssetManagementApplication();
        services.AddAssetManagementInfrastructure(connectionString);
        services.AddFaultManagementApplication();
        services.AddFaultManagementInfrastructure(connectionString);
        services.AddMaintenanceManagementApplication();
        services.AddMaintenanceManagementInfrastructure(connectionString);
        return services;
    }

    public static WebApplication UseAmmsApiDocumentation(this WebApplication app)
    {
        if (!app.Environment.IsApiDocumentationEnabled())
        {
            return app;
        }

        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.AddDocument("v1", "AMMS API");
        });
        app.MapGet("/", () => Results.Redirect("/scalar")).ExcludeFromDescription();
        return app;
    }
}
