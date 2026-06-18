using AMMS.Core.Interfaces;
using AMMS.Infrastructure.Localization;
using AMMS.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AMMS.Infrastructure
{


    public static class DependencyInjection
    {
        public static IServiceCollection AddAmmsInfrastructure(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<ICurrentOrganizationService, CurrentOrganizationService>();
            services.AddSingleton<JsonStringLocalizer>();
            return services;
        }
    }



}
