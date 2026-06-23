using AMMS.Infrastructure.Middleware;
using Microsoft.AspNetCore.Http;

namespace AMMS.Infrastructure.Logging;

public static class CorrelationIdResolver
{
    public static string? Resolve(HttpContext? context)
    {
        if (context is null)
        {
            return null;
        }

        return context.Items[CorrelationIdConstants.HttpContextItemKey] as string
            ?? context.Request.Headers[CorrelationIdConstants.HeaderName].FirstOrDefault()
            ?? context.TraceIdentifier;
    }
}
