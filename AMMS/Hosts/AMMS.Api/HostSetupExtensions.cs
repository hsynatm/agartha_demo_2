using AMMS.Infrastructure;
using AMMS.Infrastructure.Auditing;
using AMMS.Infrastructure.Logging;
using AssetManagement.Application;
using AssetManagement.Infrastructure;
using AssetManagement.Infrastructure.Persistence;
using FaultManagement.Application;
using FaultManagement.Infrastructure;
using FaultManagement.Infrastructure.Persistence;
using MaintenanceManagement.Application;
using MaintenanceManagement.Infrastructure;
using MaintenanceManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
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

    public static async Task ApplyDevelopmentMigrationsAsync(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return;
        }

        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        await services.GetRequiredService<AuditDbContext>().Database.MigrateAsync();
        await services.GetRequiredService<AssetManagementDbContext>().Database.MigrateAsync();
        await services.GetRequiredService<FaultManagementDbContext>().Database.MigrateAsync();
        await services.GetRequiredService<MaintenanceManagementDbContext>().Database.MigrateAsync();

        app.Logger.LogInformation("Development database migrations applied.");
    }
}
