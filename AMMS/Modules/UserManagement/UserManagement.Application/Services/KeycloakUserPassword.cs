namespace UserManagement.Application.Services;

/// <summary>
/// Default Keycloak password for startup bootstrap when a DB user has no Keycloak account yet.
/// API Create/Update flows use the password supplied by the client instead.
/// </summary>
public static class KeycloakUserPassword
{
    public static string FromUsername(string username) => username.Trim();
}
