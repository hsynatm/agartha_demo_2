using Microsoft.Extensions.DependencyInjection;

namespace AMMS.Infrastructure.Auditing;

public sealed class AuditInterceptorFactory
{
    public AuditInterceptor Create(IServiceProvider serviceProvider, string moduleName)
    {
        if (string.IsNullOrWhiteSpace(moduleName))
        {
            throw new ArgumentException("Module name is required.", nameof(moduleName));
        }

        return ActivatorUtilities.CreateInstance<AuditInterceptor>(serviceProvider, moduleName);
    }
}
