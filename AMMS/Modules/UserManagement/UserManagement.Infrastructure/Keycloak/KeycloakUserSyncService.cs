using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserManagement.Application.Services;
using UserManagement.Domain.Entities;

namespace UserManagement.Infrastructure.Keycloak;

public sealed class KeycloakUserSyncService : IKeycloakUserSyncService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _httpClient;
    private readonly KeycloakAdminOptions _options;
    private readonly ILogger<KeycloakUserSyncService> _logger;

    public KeycloakUserSyncService(HttpClient httpClient,IOptions<KeycloakAdminOptions> options,ILogger<KeycloakUserSyncService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> CreateUserAsync(AppUser user, string password, CancellationToken cancellationToken = default)
    {
        var token = await GetAdminTokenAsync(cancellationToken);
        var payload = BuildUserRepresentation(user);

        using var request = new HttpRequestMessage(HttpMethod.Post, AdminUsersUrl());
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(payload, options: JsonOptions);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            var existingUserId = await FindUserIdByUsernameInternalAsync(user.Username, token, cancellationToken);
            if (string.IsNullOrWhiteSpace(existingUserId))
            {
                throw CreateKeycloakException("create", user.Username, response.StatusCode, "User already exists in Keycloak.");
            }

            _logger.LogWarning(
                "Keycloak user {Username} already exists; reusing existing account {UserId}.",
                user.Username,
                existingUserId);

            await UpdateUserAsync(user, password, cancellationToken);
            return existingUserId;
        }

        if (response.StatusCode != HttpStatusCode.Created)
        {
            throw await CreateKeycloakExceptionAsync("create", user.Username, response, cancellationToken);
        }

        var keycloakUserId = ExtractUserIdFromLocation(response.Headers.Location)
            ?? throw new InvalidOperationException("Keycloak did not return a user id.");

        await SetPasswordAsync(keycloakUserId, password, token, cancellationToken);
        return keycloakUserId;
    }

    public async Task UpdateUserAsync(AppUser user, string? newPassword, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(user.KeycloakUserId))
        {
            var tokenForLookup = await GetAdminTokenAsync(cancellationToken);
            user.KeycloakUserId = await FindUserIdByUsernameInternalAsync(user.Username, tokenForLookup, cancellationToken);
            if (string.IsNullOrWhiteSpace(user.KeycloakUserId))
            {
                throw new InvalidOperationException("User is not linked to Keycloak.");
            }
        }

        var token = await GetAdminTokenAsync(cancellationToken);
        var payload = BuildUserRepresentation(user);

        using var request = new HttpRequestMessage(HttpMethod.Put, AdminUserUrl(user.KeycloakUserId));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(payload, options: JsonOptions);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw await CreateKeycloakExceptionAsync("update", user.Username, response, cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(newPassword))
        {
            await SetPasswordAsync(user.KeycloakUserId, newPassword, token, cancellationToken);
        }
    }

    public async Task DeleteUserAsync(string keycloakUserId, CancellationToken cancellationToken = default)
    {
        var token = await GetAdminTokenAsync(cancellationToken);

        using var request = new HttpRequestMessage(HttpMethod.Delete, AdminUserUrl(keycloakUserId));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode is HttpStatusCode.NoContent or HttpStatusCode.NotFound)
        {
            return;
        }

        throw await CreateKeycloakExceptionAsync("delete", keycloakUserId, response, cancellationToken);
    }

    public async Task<bool> UserExistsAsync(string keycloakUserId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(keycloakUserId))
        {
            return false;
        }

        var token = await GetAdminTokenAsync(cancellationToken);
        using var request = new HttpRequestMessage(HttpMethod.Get, AdminUserUrl(keycloakUserId));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public Task<string?> FindUserIdByUsernameAsync(string username, CancellationToken cancellationToken = default) =>
        FindUserIdByUsernameInternalAsync(username, cancellationToken);

    private async Task<string> GetAdminTokenAsync(CancellationToken cancellationToken)
    {
        var tokenUrl = $"{_options.BaseUrl.TrimEnd('/')}/realms/{_options.Realm}/protocol/openid-connect/token";
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret
        });

        using var response = await _httpClient.PostAsync(tokenUrl, content, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Keycloak token request failed: {Status} {Body}", response.StatusCode, body);
            throw new InvalidOperationException("Keycloak admin token request failed.");
        }

        var tokenResponse = await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Keycloak token response was empty.");

        return tokenResponse.AccessToken;
    }

    private async Task<string?> FindUserIdByUsernameInternalAsync(string username,CancellationToken cancellationToken)
    {
        var token = await GetAdminTokenAsync(cancellationToken);
        return await FindUserIdByUsernameInternalAsync(username, token, cancellationToken);
    }

    private async Task<string?> FindUserIdByUsernameInternalAsync(string username,string token,CancellationToken cancellationToken)
    {
        var url = $"{AdminUsersUrl()}?username={Uri.EscapeDataString(username)}&exact=true";
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var users = await response.Content.ReadFromJsonAsync<List<KeycloakUserLookup>>(cancellationToken) ?? [];
        return users.FirstOrDefault()?.Id;
    }

    private async Task SetPasswordAsync(string keycloakUserId,string password,string token,CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put,$"{AdminUserUrl(keycloakUserId)}/reset-password");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(new { type = "password", value = password, temporary = false }, options: JsonOptions);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)        
            throw await CreateKeycloakExceptionAsync("reset-password", keycloakUserId, response, cancellationToken);
        
    }

    private KeycloakUserRepresentation BuildUserRepresentation(AppUser user) =>
        new()
        {
            Username = user.Username,
            Email = user.Email.Trim(),
            FirstName = user.FirstName.Trim(),
            LastName = user.LastName.Trim(),
            Enabled = user.IsActive,
            EmailVerified = true,
            Attributes = new Dictionary<string, string[]>
            {
                ["organization_id"] = [user.OrganizationId.Trim()],
                ["organization_name"] = [user.OrganizationName.Trim()]
            }
        };

    private string AdminUsersUrl() =>
        $"{_options.BaseUrl.TrimEnd('/')}/admin/realms/{_options.Realm}/users";

    private string AdminUserUrl(string keycloakUserId) =>
        $"{AdminUsersUrl()}/{keycloakUserId}";

    private async Task<InvalidOperationException> CreateKeycloakExceptionAsync(string operation,string subject,HttpResponseMessage response,CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogError(
            "Keycloak user {Operation} failed for {Subject}: {Status} {Body}",
            operation,
            subject,
            response.StatusCode,
            body);

        return CreateKeycloakException(operation, subject, response.StatusCode, ParseErrorMessage(body));
    }

    private static InvalidOperationException CreateKeycloakException(string operation,string subject,HttpStatusCode statusCode, string detail) =>
        new($"Keycloak user {operation} failed for '{subject}': {statusCode}. {detail}");

    private static string ParseErrorMessage(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return "No error details returned by Keycloak.";
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            if (document.RootElement.TryGetProperty("errorMessage", out var errorMessage))
            {
                return errorMessage.GetString() ?? body;
            }

            if (document.RootElement.TryGetProperty("error", out var error))
            {
                return error.GetString() ?? body;
            }
        }
        catch (JsonException)
        {
            // Use raw body below.
        }

        return body;
    }

    private static string? ExtractUserIdFromLocation(Uri? location)
    {
        if (location is null)
        {
            return null;
        }

        var segments = location.Segments;
        return segments.Length == 0 ? null : segments[^1].TrimEnd('/');
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

    private sealed class KeycloakUserRepresentation
    {
        public string Username { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public bool Enabled { get; set; }

        public bool EmailVerified { get; set; }

        public Dictionary<string, string[]> Attributes { get; set; } = [];
    }
}
