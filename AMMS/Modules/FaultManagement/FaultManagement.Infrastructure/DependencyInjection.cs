using AMMS.Infrastructure.Auditing;
using FaultManagement.Domain.Persistence;
using FaultManagement.Domain.Repositories;
using FaultManagement.Infrastructure.Persistence;
using FaultManagement.Infrastructure.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace FaultManagement.Infrastructure;

public static class DependencyInjection
{
    public const string SchemaName = "FaultManagement";
    public const string ModuleName = "FaultManagement";

    public static IServiceCollection AddFaultManagementInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddAmmsModuleDbContext<FaultManagementDbContext>(connectionString, ModuleName);

        services.AddScoped<IFaultManagementRepository, FaultManagementRepository>();
        services.AddScoped<IFaultManagementUnitOfWork, FaultManagementUnitOfWork>();

        return services;
    }
}
