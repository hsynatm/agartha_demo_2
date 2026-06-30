namespace UserManagement.Infrastructure.Keycloak;

public sealed class KeycloakBootstrapOptions
{
    public const string SectionName = "KeycloakBootstrap";

    /// <summary>
    /// When enabled, dev bootstrap runs on API startup (admin user seed + Keycloak sync).
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// When true, existing Keycloak accounts get password reset to username on API startup (dev only).
    /// </summary>
    public bool ResetPasswordOnReconcile { get; set; }

    /// <summary>
    /// When true, creates the default admin user in UserManagement.Users if missing (password = username via Keycloak sync).
    /// </summary>
    public bool EnsureAdminUser { get; set; } = true;

    public string AdminUsername { get; set; } = "admin";
}
