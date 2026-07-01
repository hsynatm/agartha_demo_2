using AMMS.Infrastructure.Auditing;
using UserManagement.Application.Services;
using UserManagement.Domain.Persistence;
using UserManagement.Domain.Repositories;
using UserManagement.Infrastructure.Keycloak;
using UserManagement.Infrastructure.Persistence;
using UserManagement.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace UserManagement.Infrastructure;

public static class DependencyInjection
{
    public const string SchemaName = "UserManagement";
    public const string ModuleName = "UserManagement";

    public static IServiceCollection AddUserManagementInfrastructure(this IServiceCollection services,string connectionString,IConfiguration configuration)
    {
        services.Configure<KeycloakAdminOptions>(configuration.GetSection(KeycloakAdminOptions.SectionName));
        services.Configure<KeycloakBootstrapOptions>(configuration.GetSection(KeycloakBootstrapOptions.SectionName));
        services.AddDbContext<UserManagementDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(connectionString);
            options.UseAmmsAuditInterceptor(serviceProvider, ModuleName);
        });

        services.AddScoped<IUserManagementRepository, UserManagementRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRoleGroupRepository, RoleGroupRepository>();
        services.AddScoped<IUserManagementUnitOfWork, UserManagementUnitOfWork>();
        services.AddHttpClient<IKeycloakUserSyncService, KeycloakUserSyncService>();
        services.AddSingleton<AdminUserBootstrapRunner>();
        services.AddSingleton<KeycloakUserBootstrapRunner>();

        return services;
    }
}
