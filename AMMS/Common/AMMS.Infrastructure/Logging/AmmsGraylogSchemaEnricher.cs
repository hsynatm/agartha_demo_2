using AMMS.Core.Interfaces;
using AMMS.Shared.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog.Core;
using Serilog.Events;
using System.Globalization;

namespace AMMS.Infrastructure.Logging;

public sealed class AmmsGraylogSchemaEnricher : ILogEventEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly string _source;

    public AmmsGraylogSchemaEnricher(
        IHttpContextAccessor httpContextAccessor,
        IHostEnvironment hostEnvironment)
    {
        _httpContextAccessor = httpContextAccessor;
        _hostEnvironment = hostEnvironment;
        _source = Environment.MachineName;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var schema = AmmsLogSchemaContext.Get(httpContext);

        var application = _hostEnvironment.ApplicationName ?? GraylogSchemaDefaults.String;
        var environmentName = _hostEnvironment.EnvironmentName ?? GraylogSchemaDefaults.String;
        var traceId = httpContext?.TraceIdentifier ?? GetScalarString(logEvent, LogPropertyNames.TraceId, GraylogSchemaDefaults.String);
        var correlationId = CorrelationIdResolver.Resolve(httpContext) ?? GetScalarString(logEvent, LogPropertyNames.CorrelationId, GraylogSchemaDefaults.String);
        var clientIp = httpContext?.Connection.RemoteIpAddress?.ToString() ?? GetScalarString(logEvent, LogPropertyNames.ClientIp, GraylogSchemaDefaults.String);
        var requestMethod = httpContext?.Request.Method ?? GetScalarString(logEvent, LogPropertyNames.RequestMethod, GraylogSchemaDefaults.String);
        var requestPath = httpContext?.Request.Path.Value ?? GetScalarString(logEvent, LogPropertyNames.RequestPath, GraylogSchemaDefaults.String);
        var moduleName = ApiModuleNameResolver.ResolveFromPath(requestPath) ?? GetScalarString(logEvent, LogPropertyNames.ModuleName, GraylogSchemaDefaults.String);
        var host = httpContext?.Request.Host.Value ?? GraylogSchemaDefaults.String;
        var scheme = httpContext?.Request.Scheme ?? GraylogSchemaDefaults.String;
        var statusCode = ResolveStatusCode(logEvent, httpContext, schema);
        var elapsed = ResolveElapsed(logEvent, schema);
        var elapsedMilliseconds = ResolveElapsedMilliseconds(logEvent, schema, elapsed);
        var tenantId = ResolveTenantId(httpContext, logEvent);
        var sourceContext = ResolveSourceContext(logEvent);
        var errorCode = ResolveString(logEvent, LogPropertyNames.ErrorCode, schema.ErrorCode);
        var localizationKey = ResolveString(logEvent, LogPropertyNames.LocalizationKey, schema.LocalizationKey);
        var title = ResolveString(logEvent, LogPropertyNames.Title, schema.Title);
        var detail = ResolveString(logEvent, LogPropertyNames.Detail, schema.Detail);
        var connectionId = httpContext?.Features.Get<IHttpConnectionFeature>()?.ConnectionId ?? GraylogSchemaDefaults.String;
        var requestId = traceId;
        var stringLevel = logEvent.Level.ToString();
        var message = logEvent.RenderMessage(CultureInfo.InvariantCulture);
        if (string.IsNullOrWhiteSpace(message))
        {
            message = GraylogSchemaDefaults.String;
        }

        SetProperty(logEvent, propertyFactory, LogPropertyNames.Application, application);
        SetProperty(logEvent, propertyFactory, LogPropertyNames.EnvironmentName, environmentName);
        SetProperty(logEvent, propertyFactory, LogPropertyNames.TraceId, traceId);
        SetProperty(logEvent, propertyFactory, LogPropertyNames.CorrelationId, correlationId);
        SetProperty(logEvent, propertyFactory, LogPropertyNames.ClientIp, clientIp);
        SetProperty(logEvent, propertyFactory, LogPropertyNames.RequestMethod, requestMethod);
        SetProperty(logEvent, propertyFactory, LogPropertyNames.RequestPath, requestPath);
        SetProperty(logEvent, propertyFactory, LogPropertyNames.ModuleName, moduleName);
        SetProperty(logEvent, propertyFactory, LogPropertyNames.Host, host);
        SetProperty(logEvent, propertyFactory, LogPropertyNames.Scheme, scheme);
        SetProperty(logEvent, propertyFactory, LogPropertyNames.StatusCode, statusCode);
        SetProperty(logEvent, propertyFactory, LogPropertyNames.Elapsed, elapsed);
        SetProperty(logEvent, propertyFactory, LogPropertyNames.ElapsedMilliseconds, elapsedMilliseconds);
        SetProperty(logEvent, propertyFactory, LogPropertyNames.TenantId, tenantId);
        SetProperty(logEvent, propertyFactory, LogPropertyNames.SourceContext, sourceContext);
        SetProperty(logEvent, propertyFactory, LogPropertyNames.ErrorCode, errorCode);
        SetProperty(logEvent, propertyFactory, LogPropertyNames.LocalizationKey, localizationKey);
        SetProperty(logEvent, propertyFactory, LogPropertyNames.Title, title);
        SetProperty(logEvent, propertyFactory, LogPropertyNames.Detail, detail);
        SetProperty(logEvent, propertyFactory, LogPropertyNames.StringLevel, stringLevel);
        SetProperty(logEvent, propertyFactory, LogPropertyNames.Message, message);
        SetProperty(logEvent, propertyFactory, LogPropertyNames.Source, _source);
        SetProperty(logEvent, propertyFactory, LogPropertyNames.Timestamp, logEvent.Timestamp.UtcDateTime.ToString("O"));
        SetProperty(logEvent, propertyFactory, LogPropertyNames.ConnectionId, connectionId);
        SetProperty(logEvent, propertyFactory, LogPropertyNames.RequestId, requestId);

        if (logEvent.Exception is null)
        {
            SetProperty(logEvent, propertyFactory, LogPropertyNames.ExceptionMessage, GraylogSchemaDefaults.String);
            SetProperty(logEvent, propertyFactory, LogPropertyNames.ExceptionSource, GraylogSchemaDefaults.String);
            SetProperty(logEvent, propertyFactory, LogPropertyNames.ExceptionType, GraylogSchemaDefaults.String);
            SetProperty(logEvent, propertyFactory, LogPropertyNames.StackTrace, GraylogSchemaDefaults.String);
            return;
        }

        SetProperty(logEvent, propertyFactory, LogPropertyNames.ExceptionMessage, logEvent.Exception.Message ?? GraylogSchemaDefaults.String);
        SetProperty(logEvent, propertyFactory, LogPropertyNames.ExceptionSource, logEvent.Exception.Source ?? GraylogSchemaDefaults.String);
        SetProperty(logEvent, propertyFactory, LogPropertyNames.ExceptionType, logEvent.Exception.GetType().FullName ?? GraylogSchemaDefaults.String);
        SetProperty(logEvent, propertyFactory, LogPropertyNames.StackTrace, logEvent.Exception.StackTrace ?? GraylogSchemaDefaults.String);
    }

    private static int ResolveStatusCode(LogEvent logEvent, HttpContext? httpContext, AmmsLogSchemaSnapshot schema)
    {
        if (schema.StatusCode > 0)
        {
            return schema.StatusCode;
        }

        var fromEvent = GetScalarInt(logEvent, LogPropertyNames.StatusCode);
        if (fromEvent > 0)
        {
            return fromEvent;
        }

        if (httpContext?.Response.HasStarted == true || httpContext?.Response.StatusCode > 0)
        {
            return httpContext.Response.StatusCode;
        }

        return GraylogSchemaDefaults.StatusCode;
    }

    private static double ResolveElapsed(LogEvent logEvent, AmmsLogSchemaSnapshot schema)
    {
        if (schema.Elapsed > 0)
        {
            return schema.Elapsed;
        }

        return GetScalarDouble(logEvent, LogPropertyNames.Elapsed, GraylogSchemaDefaults.Elapsed);
    }

    private static long ResolveElapsedMilliseconds(LogEvent logEvent, AmmsLogSchemaSnapshot schema, double elapsed)
    {
        if (schema.ElapsedMilliseconds > 0)
        {
            return schema.ElapsedMilliseconds;
        }

        var fromEvent = GetScalarLong(logEvent, LogPropertyNames.ElapsedMilliseconds);
        if (fromEvent > 0)
        {
            return fromEvent;
        }

        return elapsed > 0 ? (long)elapsed : GraylogSchemaDefaults.ElapsedMilliseconds;
    }

    private static string ResolveTenantId(HttpContext? httpContext, LogEvent logEvent)
    {
        var organizationService = httpContext?.RequestServices.GetService(typeof(ICurrentOrganizationService)) as ICurrentOrganizationService;
        var tenantId = organizationService?.CurrentOrganization?.OrganizationId;
        if (!string.IsNullOrWhiteSpace(tenantId))
        {
            return tenantId;
        }

        return GetScalarString(logEvent, LogPropertyNames.TenantId, GraylogSchemaDefaults.String);
    }

    private static string ResolveSourceContext(LogEvent logEvent)
    {
        if (logEvent.Properties.TryGetValue("SourceContext", out var sourceContext))
        {
            return sourceContext.ToString().Trim('"');
        }

        return GraylogSchemaDefaults.String;
    }

    private static string ResolveString(LogEvent logEvent, string propertyName, string schemaValue)
    {
        if (!string.Equals(schemaValue, GraylogSchemaDefaults.String, StringComparison.Ordinal))
        {
            return schemaValue;
        }

        return GetScalarString(logEvent, propertyName, GraylogSchemaDefaults.String);
    }

    private static void SetProperty(LogEvent logEvent, ILogEventPropertyFactory propertyFactory, string name, object? value)
    {
        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(name, value ?? GraylogSchemaDefaults.String));
    }

    private static string GetScalarString(LogEvent logEvent, string name, string defaultValue)
    {
        if (!logEvent.Properties.TryGetValue(name, out var property))
        {
            return defaultValue;
        }

        if (property is ScalarValue scalar && scalar.Value is not null)
        {
            return scalar.Value.ToString() ?? defaultValue;
        }

        var rendered = property.ToString().Trim('"');
        return string.IsNullOrWhiteSpace(rendered) ? defaultValue : rendered;
    }

    private static int GetScalarInt(LogEvent logEvent, string name)
    {
        if (!logEvent.Properties.TryGetValue(name, out var property) || property is not ScalarValue scalar || scalar.Value is null)
        {
            return GraylogSchemaDefaults.StatusCode;
        }

        return scalar.Value switch
        {
            int intValue => intValue,
            long longValue => (int)longValue,
            double doubleValue => (int)doubleValue,
            _ when int.TryParse(scalar.Value.ToString(), out var parsed) => parsed,
            _ => GraylogSchemaDefaults.StatusCode
        };
    }

    private static long GetScalarLong(LogEvent logEvent, string name)
    {
        if (!logEvent.Properties.TryGetValue(name, out var property) || property is not ScalarValue scalar || scalar.Value is null)
        {
            return GraylogSchemaDefaults.ElapsedMilliseconds;
        }

        return scalar.Value switch
        {
            long longValue => longValue,
            int intValue => intValue,
            double doubleValue => (long)doubleValue,
            _ when long.TryParse(scalar.Value.ToString(), out var parsed) => parsed,
            _ => GraylogSchemaDefaults.ElapsedMilliseconds
        };
    }

    private static double GetScalarDouble(LogEvent logEvent, string name, double defaultValue)
    {
        if (!logEvent.Properties.TryGetValue(name, out var property) || property is not ScalarValue scalar || scalar.Value is null)
        {
            return defaultValue;
        }

        return scalar.Value switch
        {
            double doubleValue => doubleValue,
            float floatValue => floatValue,
            long longValue => longValue,
            int intValue => intValue,
            _ when double.TryParse(scalar.Value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) => parsed,
            _ => defaultValue
        };
    }
}
