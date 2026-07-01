using Microsoft.AspNetCore.Http;

namespace AMMS.Infrastructure.Authentication;

public sealed record AuthorizationFailureSnapshot(
    string Reason,
    string? KeycloakUserId = null,
    string? RequiredRoles = null,
    string? RequiredRoleGroups = null);

public static class AuthorizationFailureContext
{
    public const string HttpContextItemKey = "Amms.AuthorizationFailure";

    public static void Set(HttpContext? context, AuthorizationFailureSnapshot snapshot)
    {
        if (context is null)
        {
            return;
        }

        context.Items[HttpContextItemKey] = snapshot;
    }

    public static AuthorizationFailureSnapshot? Get(HttpContext? context) =>context?.Items[HttpContextItemKey] as AuthorizationFailureSnapshot;
}
