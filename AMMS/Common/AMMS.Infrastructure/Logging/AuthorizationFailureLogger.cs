using AMMS.Core.Exceptions;
using AMMS.Infrastructure.Authentication;
using AMMS.Shared.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AMMS.Infrastructure.Logging;

public sealed class AuthorizationFailureLogger
{
    private readonly ILogger<AuthorizationFailureLogger> _logger;

    public AuthorizationFailureLogger(ILogger<AuthorizationFailureLogger> logger)
    {
        _logger = logger;
    }

    public void Log(HttpContext context, int statusCode, string errorCode, string detail, Exception? exception = null)
    {
        var failure = AuthorizationFailureContext.Get(context);
        var keycloakUserId = failure?.KeycloakUserId ?? KeycloakClaims.GetKeycloakUserId(context.User);
        var path = context.Request.Path.Value ?? GraylogSchemaDefaults.String;
        var reason = failure?.Reason ?? detail;
        var authException = exception ?? CreateAuthorizationException(statusCode, reason);

        _logger.LogWarning(
            authException,
            "Authorization failure. ErrorCode={ErrorCode} StatusCode={StatusCode} Reason={AuthorizationReason} KeycloakUserId={KeycloakUserId} RequiredRoles={RequiredRoles} RequiredRoleGroups={RequiredRoleGroups} RequestPath={RequestPath}",
            errorCode,
            statusCode,
            reason,
            keycloakUserId ?? GraylogSchemaDefaults.String,
            failure?.RequiredRoles ?? GraylogSchemaDefaults.String,
            failure?.RequiredRoleGroups ?? GraylogSchemaDefaults.String,
            path);
    }

    private static Exception CreateAuthorizationException(int statusCode, string reason) =>
        statusCode switch
        {
            StatusCodes.Status401Unauthorized => new AmmsException.Unauthorized(details: new { reason }),
            StatusCodes.Status403Forbidden => new AmmsException.Forbidden(details: new { reason }),
            _ => new UnauthorizedAccessException(reason)
        };
}
