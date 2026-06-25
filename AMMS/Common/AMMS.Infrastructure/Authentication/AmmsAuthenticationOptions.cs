namespace AMMS.Infrastructure.Authentication;

public sealed class AmmsAuthenticationOptions
{
    public const string SectionName = "Authentication";

    public string Authority { get; set; } = string.Empty;

    public string Audience { get; set; } = "amms-spa";

    public bool RequireHttpsMetadata { get; set; } = true;

    public string OrganizationClaimType { get; set; } = "organization_id";

    public string OrganizationNameClaimType { get; set; } = "organization_name";

    public string SpaClientId { get; set; } = "amms-spa";
}
