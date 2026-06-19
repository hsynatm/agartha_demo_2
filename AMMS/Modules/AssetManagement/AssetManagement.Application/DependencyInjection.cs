using AssetManagement.Application.Mappings;
using AssetManagement.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AssetManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddAssetManagementApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => cfg.AddProfile<AssetMapping>());
        services.AddScoped<IAssetManagementService, AssetManagementService>();
        return services;
    }
}
