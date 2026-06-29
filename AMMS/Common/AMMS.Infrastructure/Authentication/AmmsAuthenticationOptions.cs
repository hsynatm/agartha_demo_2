namespace AMMS.Infrastructure.Authentication;

public sealed class AmmsAuthenticationOptions
{
    public const string SectionName = "Authentication";

    public string Authority { get; set; } = string.Empty;

    public string Audience { get; set; } = "amms-spa";

    public bool RequireHttpsMetadata { get; set; } = true;

    public string OrganizationClaimType { get; set; } = DefaultOrganizationClaimType;

    public const string DefaultOrganizationClaimType = "organization_id";

    public string OrganizationNameClaimType { get; set; } = "organization_name";

    public string SpaClientId { get; set; } = "amms-spa";

    /// <summary>
    /// When true, each request validates the access token session via Keycloak introspection (with short cache).
    /// </summary>
    public bool EnableTokenIntrospection { get; set; } = true;

    /// <summary>
    /// Cache duration for active introspection results. 0 = always ask Keycloak.
    /// Default 5 seconds balances revoke detection vs Keycloak load.
    /// </summary>
    public int IntrospectionCacheSeconds { get; set; } = 5;

    public string IntrospectionClientId { get; set; } = "amms-api";

    public string IntrospectionClientSecret { get; set; } = string.Empty;
}
