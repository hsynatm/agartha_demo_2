using System.Net.Http.Headers;
using System.Text.Json.Serialization;

namespace WebApp.Services;


public sealed class KeycloakSessionService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public KeycloakSessionService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task RevokeAllSessionsByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return;
        }

        var adminToken = await GetAdminTokenAsync(cancellationToken);
        var userId = await FindUserIdByUsernameAsync(username, adminToken, cancellationToken);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        var baseUrl = _configuration["KeycloakAdmin:BaseUrl"] ?? "http://localhost:8080";
        var realm = _configuration["KeycloakAdmin:Realm"] ?? "amms";
        var url = $"{baseUrl.TrimEnd('/')}/admin/realms/{realm}/users/{userId}/logout";

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var httpClient = _httpClientFactory.CreateClient();
        using var response = await httpClient.SendAsync(request, cancellationToken);

        // 404 = user yok veya zaten oturum yok; login devam edebilir.
        if (response.StatusCode is System.Net.HttpStatusCode.NotFound)       
            return;
        

        response.EnsureSuccessStatusCode();
    }

    private async Task<string> GetAdminTokenAsync(CancellationToken cancellationToken)
    {
        var baseUrl = _configuration["KeycloakAdmin:BaseUrl"] ?? "http://localhost:8080";
        var realm = _configuration["KeycloakAdmin:Realm"] ?? "amms";
        var clientId = _configuration["KeycloakAdmin:ClientId"] ?? "amms-api";
        var clientSecret = _configuration["KeycloakAdmin:ClientSecret"]
            ?? throw new InvalidOperationException("KeycloakAdmin:ClientSecret is not configured.");

        var tokenUrl = $"{baseUrl.TrimEnd('/')}/realms/{realm}/protocol/openid-connect/token";
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret
        });

        var httpClient = _httpClientFactory.CreateClient();
        using var response = await httpClient.PostAsync(tokenUrl, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Keycloak admin token response was empty.");

        return tokenResponse.AccessToken;
    }

    private async Task<string?> FindUserIdByUsernameAsync(string username,string adminToken,CancellationToken cancellationToken)
    {
        var baseUrl = _configuration["KeycloakAdmin:BaseUrl"] ?? "http://localhost:8080";
        var realm = _configuration["KeycloakAdmin:Realm"] ?? "amms";
        var url = $"{baseUrl.TrimEnd('/')}/admin/realms/{realm}/users?username={Uri.EscapeDataString(username)}&exact=true";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var httpClient = _httpClientFactory.CreateClient();
        using var response = await httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)       
            return null;        

        var users = await response.Content.ReadFromJsonAsync<List<KeycloakUserLookup>>(cancellationToken) ?? [];
        return users.FirstOrDefault()?.Id;
    }

    /// <summary>
    /// RFC 7662 introspection — refresh kullanmaz, SSO idle sayacını sıfırlamaz.
    /// </summary>
    public async Task<bool> IsAccessTokenActiveAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return false;
        }

        var authority = _configuration["Keycloak:Authority"] ?? "http://localhost:8080/realms/amms";
        var clientId = _configuration["KeycloakAdmin:ClientId"] ?? "amms-api";
        var clientSecret = _configuration["KeycloakAdmin:ClientSecret"];
        if (string.IsNullOrWhiteSpace(clientSecret))
        {
            return false;
        }

        var introspectionUrl = $"{authority.TrimEnd('/')}/protocol/openid-connect/token/introspect";
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["token"] = accessToken,
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret
        });

        var httpClient = _httpClientFactory.CreateClient();
        using var response = await httpClient.PostAsync(introspectionUrl, content, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await System.Text.Json.JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        return document.RootElement.TryGetProperty("active", out var activeElement)
               && activeElement.GetBoolean();
    }

    private sealed class KeycloakTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = null!;
    }

    private sealed class KeycloakUserLookup
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;
    }
}
