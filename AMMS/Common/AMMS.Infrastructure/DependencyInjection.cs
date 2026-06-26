using AMMS.Core.Interfaces;
using AMMS.Infrastructure.Authentication;
using AMMS.Infrastructure.Logging;
using AMMS.Infrastructure.Localization;
using AMMS.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AMMS.Infrastructure
{


    public static class DependencyInjection
    {
        public static IServiceCollection AddAmmsInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpContextAccessor();
            services.AddSingleton<AmmsGraylogSchemaEnricher>();
            services.AddAmmsAuthentication(configuration);
            services.AddAmmsCors(configuration);
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<ICurrentOrganizationService, CurrentOrganizationService>();
            services.AddScoped<AuthorizationFailureLogger>();
            services.AddSingleton<JsonStringLocalizer>();
            return services;
        }
    }



}
