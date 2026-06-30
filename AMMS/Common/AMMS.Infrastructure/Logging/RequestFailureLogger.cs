using AMMS.Core.Exceptions;
using AMMS.Core.Localization;
using AMMS.Infrastructure.Authentication;
using AMMS.Shared.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using System.Reflection;

namespace AMMS.Infrastructure.Logging;

public sealed class RequestFailureLogger
{
    private readonly ILogger<RequestFailureLogger> _logger;

    public RequestFailureLogger(ILogger<RequestFailureLogger> logger)
    {
        _logger = logger;
    }

    public void Log(HttpContext context, int statusCode, string errorCode, string localizationKey, string title, string detail, Exception? exception = null)
    {
        AmmsLogSchemaContext.SetFromMapped(context, errorCode, localizationKey, title, detail, statusCode);

        var path = context.Request.Path.Value ?? GraylogSchemaDefaults.String;
        var moduleName = ApiModuleNameResolver.ResolveFromPath(path) ?? GraylogSchemaDefaults.String;
        var entityId = ExtractEntityId(exception) ?? GraylogSchemaDefaults.String;
        var keycloakUserId = KeycloakClaims.GetKeycloakUserId(context.User) ?? GraylogSchemaDefaults.String;
        var logException = exception ?? CreateException(statusCode, detail);

        using (LogContext.PushProperty(LogPropertyNames.ErrorCode, errorCode))
        using (LogContext.PushProperty(LogPropertyNames.StatusCode, statusCode))
        using (LogContext.PushProperty(LogPropertyNames.LocalizationKey, localizationKey))
        using (LogContext.PushProperty(LogPropertyNames.Title, title))
        using (LogContext.PushProperty(LogPropertyNames.Detail, detail))
        using (LogContext.PushProperty(LogPropertyNames.RequestPath, path))
        using (LogContext.PushProperty(LogPropertyNames.ModuleName, moduleName))
        using (LogContext.PushProperty(LogPropertyNames.EntityId, entityId))
        using (LogContext.PushProperty(LogPropertyNames.UserId, keycloakUserId))
        {
            if (statusCode >= StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(
                    logException,
                    "Request failed. ErrorCode={ErrorCode} StatusCode={StatusCode} Detail={Detail} RequestPath={RequestPath} ModuleName={ModuleName} EntityId={EntityId} KeycloakUserId={KeycloakUserId}",
                    errorCode,
                    statusCode,
                    detail,
                    path,
                    moduleName,
                    entityId,
                    keycloakUserId);
                return;
            }

            _logger.LogWarning(
                logException,
                "Request failed. ErrorCode={ErrorCode} StatusCode={StatusCode} Detail={Detail} RequestPath={RequestPath} ModuleName={ModuleName} EntityId={EntityId} KeycloakUserId={KeycloakUserId}",
                errorCode,
                statusCode,
                detail,
                path,
                moduleName,
                entityId,
                keycloakUserId);
        }
    }

    public void LogSessionTerminated(HttpContext context, string errorCode, string message)
    {
        Log(
            context,
            StatusCodes.Status401Unauthorized,
            errorCode,
            GraylogSchemaDefaults.String,
            "Session terminated",
            message);
    }

    private static Exception CreateException(int statusCode, string detail) =>
        statusCode switch
        {
            StatusCodes.Status401Unauthorized => new AmmsException.Unauthorized(details: new { reason = detail }),
            StatusCodes.Status403Forbidden => new AmmsException.Forbidden(details: new { reason = detail }),
            StatusCodes.Status404NotFound => new AmmsException.NotFound(
                LocalizationKeys.Shared.NotFound,
                details: new { reason = detail }),
            _ => new InvalidOperationException(detail)
        };

    private static string? ExtractEntityId(Exception? exception)
    {
        if (exception is not AmmsException ammsException || ammsException.Details is null)
        {
            return null;
        }

        var entityIdProperty = ammsException.Details.GetType().GetProperty(
            "EntityId",
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

        return entityIdProperty?.GetValue(ammsException.Details)?.ToString();
    }
}
