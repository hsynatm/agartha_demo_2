using AMMS.Core.Interfaces;
using AMMS.Shared.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

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
                    .Enrich.WithProperty(LogPropertyNames.EnvironmentName, context.HostingEnvironment.EnvironmentName);
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
                    var hostEnvironment = httpContext.RequestServices.GetRequiredService<IHostEnvironment>();
                    var correlationId = httpContext.Request.Headers["X-Correlation-ID"].FirstOrDefault()
                        ?? httpContext.TraceIdentifier;

                    var requestPath = httpContext.Request.Path.Value ?? string.Empty;
                    var requestQuery = SensitiveQueryStringMasker.Mask(httpContext.Request.QueryString.Value);

                    diagnosticContext.Set(LogPropertyNames.Application, hostEnvironment.ApplicationName);
                    diagnosticContext.Set(LogPropertyNames.EnvironmentName, hostEnvironment.EnvironmentName);
                    diagnosticContext.Set(LogPropertyNames.TraceId, httpContext.TraceIdentifier);
                    diagnosticContext.Set(LogPropertyNames.CorrelationId, correlationId);
                    diagnosticContext.Set(LogPropertyNames.ClientIp, httpContext.Connection.RemoteIpAddress?.ToString());
                    diagnosticContext.Set(LogPropertyNames.RequestMethod, httpContext.Request.Method);
                    diagnosticContext.Set(LogPropertyNames.RequestPath, requestPath);
                    diagnosticContext.Set(LogPropertyNames.RequestQuery, requestQuery);
                    diagnosticContext.Set(LogPropertyNames.StatusCode, httpContext.Response.StatusCode);

                    var userService = httpContext.RequestServices.GetService<ICurrentUserService>();
                    var organizationService = httpContext.RequestServices.GetService<ICurrentOrganizationService>();

                    diagnosticContext.Set(LogPropertyNames.UserId, userService?.CurrentUser?.UserId);
                    diagnosticContext.Set(LogPropertyNames.OrganizationId, organizationService?.CurrentOrganization?.OrganizationId);
                };
            });

            return app;
        }
    }




}
