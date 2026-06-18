using AMMS.Core.Interfaces;
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

            services.AddScoped<AuditInterceptor>();

            return services;
        }

        public static IServiceCollection AddAuditModule(this IServiceCollection services, string moduleName)
        {
            if (string.IsNullOrWhiteSpace(moduleName))
            {
                throw new ArgumentException("Module name is required.", nameof(moduleName));
            }

            services.AddScoped<IAuditModuleContext>(_ => new ModuleContext(moduleName));
            return services;
        }

        public static DbContextOptionsBuilder UseAmmsAuditInterceptor(
            this DbContextOptionsBuilder options,
            IServiceProvider serviceProvider)
        {
            var auditInterceptor = serviceProvider.GetRequiredService<AuditInterceptor>();
            return options.AddInterceptors(auditInterceptor);
        }

        private sealed class ModuleContext(string moduleName) : IAuditModuleContext
        {
            public string ModuleName { get; } = moduleName;
        }
    }














}
