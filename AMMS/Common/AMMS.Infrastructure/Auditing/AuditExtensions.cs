using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AMMS.Infrastructure.Auditing
{
    public static class AuditExtensions
    {
        public static IServiceCollection AddAmmsAuditing(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<AuditDbContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddSingleton<AuditInterceptorFactory>();

            return services;
        }

        public static DbContextOptionsBuilder UseAmmsAuditInterceptor(
            this DbContextOptionsBuilder options,
            IServiceProvider serviceProvider,
            string moduleName)
        {
            var auditInterceptorFactory = serviceProvider.GetRequiredService<AuditInterceptorFactory>();
            return options.AddInterceptors(auditInterceptorFactory.Create(serviceProvider, moduleName));
        }
    }
}
