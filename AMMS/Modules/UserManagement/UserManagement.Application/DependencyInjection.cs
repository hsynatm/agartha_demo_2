using AMMS.Core.Interfaces;
using UserManagement.Application.Mappings;
using UserManagement.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace UserManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddUserManagementApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => cfg.AddProfile<UserMapping>());
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IRoleGroupService, RoleGroupService>();
        services.AddScoped<IUserAuthorizationService, UserAuthorizationService>();
        return services;
    }
}
