namespace UserManagement.Infrastructure.Keycloak;

public sealed class KeycloakAdminOptions
{
    public const string SectionName = "KeycloakAdmin";

    public string BaseUrl { get; set; } = "http://localhost:8080";

    public string Realm { get; set; } = "amms";

    public string ClientId { get; set; } = "amms-api";

    public string ClientSecret { get; set; } = string.Empty;
}
