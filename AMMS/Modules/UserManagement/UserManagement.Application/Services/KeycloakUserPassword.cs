namespace UserManagement.Application.Services;

/// <summary>
/// Keycloak identity password policy: password equals username.
/// Roles remain in PostgreSQL; this applies to Keycloak login only.
/// </summary>
public static class KeycloakUserPassword
{
    public static string FromUsername(string username) => username.Trim();
}
