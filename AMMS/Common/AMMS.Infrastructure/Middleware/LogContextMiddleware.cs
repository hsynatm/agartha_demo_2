using AMMS.Core.Interfaces;
using AMMS.Infrastructure.Logging;
using AMMS.Shared.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Serilog.Context;

namespace AMMS.Infrastructure.Middleware
{

    public sealed class LogContextMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _applicationName;
        private readonly string _environmentName;

        public LogContextMiddleware(RequestDelegate next, IHostEnvironment hostEnvironment)
        {
            _next = next;
            _applicationName = hostEnvironment.ApplicationName;
            _environmentName = hostEnvironment.EnvironmentName;
        }

        public async Task InvokeAsync(
            HttpContext context,
            ICurrentUserService currentUserService,
            ICurrentOrganizationService currentOrganizationService)
        {
            var traceId = context.TraceIdentifier;
            var clientIp = context.Connection.RemoteIpAddress?.ToString();
            var requestPath = context.Request.Path.Value ?? string.Empty;
            var requestQuery = SensitiveQueryStringMasker.Mask(context.Request.QueryString.Value);
            var correlationId = context.Items[CorrelationIdConstants.HttpContextItemKey] as string
                ?? context.Request.Headers[CorrelationIdConstants.HeaderName].FirstOrDefault()
                ?? traceId;
            var moduleName = ApiModuleNameResolver.ResolveFromPath(requestPath);

            var userId = currentUserService.CurrentUser?.UserId;
            var tenantId = currentOrganizationService.CurrentOrganization?.OrganizationId;

            using (LogContext.PushProperty(LogPropertyNames.Application, _applicationName))
            using (LogContext.PushProperty(LogPropertyNames.EnvironmentName, _environmentName))
            using (LogContext.PushProperty(LogPropertyNames.TraceId, traceId))
            using (LogContext.PushProperty(LogPropertyNames.CorrelationId, correlationId))
            using (LogContext.PushProperty(LogPropertyNames.ClientIp, clientIp))
            using (LogContext.PushProperty(LogPropertyNames.RequestPath, requestPath))
            using (LogContext.PushProperty(LogPropertyNames.RequestQuery, requestQuery))
            using (LogContext.PushProperty(LogPropertyNames.RequestMethod, context.Request.Method))
            using (LogContext.PushProperty(LogPropertyNames.UserId, userId))
            using (LogContext.PushProperty(LogPropertyNames.TenantId, tenantId))
            using (LogContext.PushProperty(LogPropertyNames.ModuleName, moduleName))
            {
                await _next(context);
            }
        }
    }
}
