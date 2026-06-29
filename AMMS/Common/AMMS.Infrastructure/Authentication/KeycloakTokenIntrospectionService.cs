using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace AMMS.Infrastructure.Authentication;

/// <summary>
/// RFC 7662 token introspection against Keycloak.
/// JWT signature is validated locally first; introspection confirms the session is still active.
/// </summary>
public sealed class KeycloakTokenIntrospectionService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AmmsAuthenticationOptions _options;
    private readonly IMemoryCache _cache;

    public KeycloakTokenIntrospectionService(
        IHttpClientFactory httpClientFactory,
        IOptions<AmmsAuthenticationOptions> options,
        IMemoryCache cache)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _cache = cache;
    }

    public async Task<bool> IsTokenActiveAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        if (!_options.EnableTokenIntrospection)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return false;
        }

        var cacheKey = "kc-introspect:" + ComputeTokenHash(accessToken);
        if (_options.IntrospectionCacheSeconds > 0
            && _cache.TryGetValue(cacheKey, out bool cachedActive))
        {
            return cachedActive;
        }

        var active = await IntrospectFromKeycloakAsync(accessToken, cancellationToken);

        if (active && _options.IntrospectionCacheSeconds > 0)
        {
            _cache.Set(cacheKey, true, TimeSpan.FromSeconds(_options.IntrospectionCacheSeconds));
        }

        return active;
    }

    private async Task<bool> IntrospectFromKeycloakAsync(string accessToken, CancellationToken cancellationToken)
    {
        var introspectionUrl = $"{_options.Authority.TrimEnd('/')}/protocol/openid-connect/token/introspect";

        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["token"] = accessToken,
            ["client_id"] = _options.IntrospectionClientId,
            ["client_secret"] = _options.IntrospectionClientSecret
        });

        var httpClient = _httpClientFactory.CreateClient();
        using var response = await httpClient.PostAsync(introspectionUrl, content, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        return document.RootElement.TryGetProperty("active", out var activeElement)
               && activeElement.GetBoolean();
    }

    private static string ComputeTokenHash(string accessToken)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(accessToken));
        return Convert.ToHexString(hash);
    }
}
