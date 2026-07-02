using AMMS.Infrastructure.Auditing;
using AssetManagement.Domain.Persistence;
using AssetManagement.Domain.Repositories;
using AssetManagement.Infrastructure.Persistence;
using AssetManagement.Infrastructure.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace AssetManagement.Infrastructure;

public static class DependencyInjection
{
    public const string SchemaName = "AssetManagement";
    public const string ModuleName = "AssetManagement";

    public static IServiceCollection AddAssetManagementInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddAmmsModuleDbContext<AssetManagementDbContext>(connectionString, ModuleName);

        services.AddScoped<IAssetManagementRepository, AssetManagementRepository>();
        services.AddScoped<IAssetManagementUnitOfWork, AssetManagementUnitOfWork>();

        return services;
    }
}
