using AMMS.Infrastructure;
using AMMS.Infrastructure.Auditing;
using AMMS.Infrastructure.Logging;
using AssetManagement.Application;
using AssetManagement.Infrastructure;
using FaultManagement.Application;
using FaultManagement.Infrastructure;
using MaintenanceManagement.Application;
using MaintenanceManagement.Infrastructure;
using UserManagement.Application;
using UserManagement.Infrastructure;

namespace AMMS.Api;

internal static class HostSetupExtensions
{
    public static bool IsApiDocumentationEnabled(this IHostEnvironment environment) =>
        environment.IsDevelopment() || environment.IsEnvironment("Test");

    public static IServiceCollection AddAmmsModules(this IServiceCollection services, string connectionString, IConfiguration configuration)
    {
        services.AddAmmsInfrastructure(configuration);
        services.AddAmmsAuditing(connectionString);
        services.AddAssetManagementApplication();
        services.AddAssetManagementInfrastructure(connectionString);
        services.AddFaultManagementApplication();
        services.AddFaultManagementInfrastructure(connectionString);
        services.AddMaintenanceManagementApplication();
        services.AddMaintenanceManagementInfrastructure(connectionString);
        services.AddUserManagementApplication();
        services.AddUserManagementInfrastructure(connectionString, configuration);
        return services;
    }

    public static WebApplication UseAmmsApiDocumentation(this WebApplication app)
    {
        if (!app.Environment.IsApiDocumentationEnabled())
        {
            return app;
        }

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "AMMS API v1");
            options.RoutePrefix = "swagger";
        });
        app.MapGet("/", () => Results.Redirect("/swagger"))
            .AllowAnonymous()
            .ExcludeFromDescription();
        return app;
    }
}
