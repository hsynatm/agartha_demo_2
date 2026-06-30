using AMMS.Infrastructure.Authentication;
using AMMS.Shared.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System.Security.Claims;

namespace AMMS.Infrastructure.Logging
{
    public static class SerilogExtensions
    {
        public static WebApplicationBuilder AddSerilogLogging(this WebApplicationBuilder builder)
        {
            builder.Host.UseSerilog((context, services, loggerConfiguration) =>
            {
                loggerConfiguration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty(LogPropertyNames.Application, context.HostingEnvironment.ApplicationName)
                    .Enrich.WithProperty(LogPropertyNames.EnvironmentName, context.HostingEnvironment.EnvironmentName)
                    .Enrich.With(services.GetRequiredService<AmmsGraylogSchemaEnricher>())
                    .AddAmmsGraylog(context.Configuration, context.HostingEnvironment);
            });

            return builder;
        }

        public static WebApplication UseAmmsRequestLogging(this WebApplication app)
        {
            app.UseSerilogRequestLogging(options =>
            {
                options.MessageTemplate =
                    "[{Application}] {RequestMethod} {RequestPath} => {StatusCode} ({Elapsed:0.0000}ms)";

                options.GetLevel = (httpContext, elapsed, exception) =>
                {
                    if (exception is not null || httpContext.Response.StatusCode >= 500)
                    {
                        return LogEventLevel.Error;
                    }

                    if (httpContext.Response.StatusCode >= 400)
                    {
                        return LogEventLevel.Warning;
                    }

                    return LogEventLevel.Information;
                };

                options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    AmmsLogSchemaContext.SetFromRequest(
                        httpContext,
                        httpContext.Response.StatusCode,
                        GraylogSchemaDefaults.Elapsed);

                    var schema = AmmsLogSchemaContext.Get(httpContext);
                    var requestPath = httpContext.Request.Path.Value ?? GraylogSchemaDefaults.String;
                    var moduleName = ApiModuleNameResolver.ResolveFromPath(requestPath) ?? GraylogSchemaDefaults.String;

                    diagnosticContext.Set(LogPropertyNames.StatusCode, schema.StatusCode);
                    diagnosticContext.Set(LogPropertyNames.Elapsed, schema.Elapsed);
                    diagnosticContext.Set(LogPropertyNames.ElapsedMilliseconds, schema.ElapsedMilliseconds);
                    diagnosticContext.Set(LogPropertyNames.ErrorCode, schema.ErrorCode);
                    diagnosticContext.Set(LogPropertyNames.LocalizationKey, schema.LocalizationKey);
                    diagnosticContext.Set(LogPropertyNames.Title, schema.Title);
                    diagnosticContext.Set(LogPropertyNames.Detail, schema.Detail);
                    diagnosticContext.Set(LogPropertyNames.RequestPath, requestPath);
                    diagnosticContext.Set(LogPropertyNames.RequestMethod, httpContext.Request.Method);
                    diagnosticContext.Set(LogPropertyNames.ModuleName, moduleName);
                    diagnosticContext.Set(LogPropertyNames.CorrelationId, CorrelationIdResolver.Resolve(httpContext) ?? GraylogSchemaDefaults.String);
                    diagnosticContext.Set(LogPropertyNames.TraceId, httpContext.TraceIdentifier);
                    diagnosticContext.Set(LogPropertyNames.ClientIp, httpContext.Connection.RemoteIpAddress?.ToString() ?? GraylogSchemaDefaults.String);
                    diagnosticContext.Set(LogPropertyNames.RequestHost, httpContext.Request.Host.Value ?? GraylogSchemaDefaults.String);
                    diagnosticContext.Set(LogPropertyNames.Scheme, httpContext.Request.Scheme ?? GraylogSchemaDefaults.String);

                    var tenantId = httpContext.User.Identity?.IsAuthenticated == true
                        ? httpContext.User.FindFirstValue(AmmsAuthenticationOptions.DefaultOrganizationClaimType)
                        : null;
                    diagnosticContext.Set(
                        LogPropertyNames.TenantId,
                        tenantId ?? GraylogSchemaDefaults.String);
                };
            });

            return app;
        }
    }
}
