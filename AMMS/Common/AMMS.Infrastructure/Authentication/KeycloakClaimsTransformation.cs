using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

namespace AMMS.Infrastructure.Authentication;

public sealed class KeycloakClaimsTransformation : IClaimsTransformation
{
    private readonly AmmsAuthenticationOptions _options;

    public KeycloakClaimsTransformation(Microsoft.Extensions.Options.IOptions<AmmsAuthenticationOptions> options)
    {
        _options = options.Value;
    }

    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
        {
            return Task.FromResult(principal);
        }

        AddRolesFromRealmAccess(identity);
        AddRolesFromResourceAccess(identity);
        AddRolesFromDirectClaims(identity);
        EnsureAuthorizedParty(identity);

        return Task.FromResult(principal);
    }

    private void AddRolesFromRealmAccess(ClaimsIdentity identity)
    {
        var realmAccess = identity.FindFirst("realm_access")?.Value;
        if (string.IsNullOrWhiteSpace(realmAccess))
        {
            return;
        }

        AddRolesFromJson(identity, realmAccess, "roles");
    }

    private void AddRolesFromResourceAccess(ClaimsIdentity identity)
    {
        var resourceAccess = identity.FindFirst("resource_access")?.Value;
        if (string.IsNullOrWhiteSpace(resourceAccess))
        {
            return;
        }

        try
        {
            using var document = JsonDocument.Parse(resourceAccess);
            foreach (var client in document.RootElement.EnumerateObject())
            {
                if (client.Value.TryGetProperty("roles", out var roles))
                {
                    AddRoleClaims(identity, roles);
                }
            }
        }
        catch (JsonException)
        {
            // Ignore malformed resource_access payloads.
        }
    }

    private static void AddRolesFromJson(ClaimsIdentity identity, string json, string propertyName)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.TryGetProperty(propertyName, out var roles))
            {
                AddRoleClaims(identity, roles);
            }
        }
        catch (JsonException)
        {
            // Ignore malformed role payloads.
        }
    }

    private static void AddRoleClaims(ClaimsIdentity identity, JsonElement roles)
    {
        if (roles.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (var role in roles.EnumerateArray())
        {
            var roleName = role.GetString();
            if (string.IsNullOrWhiteSpace(roleName))
            {
                continue;
            }

            if (!identity.HasClaim(claim => claim.Type == ClaimTypes.Role && claim.Value == roleName))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
            }
        }
    }

    private static void AddRolesFromDirectClaims(ClaimsIdentity identity)
    {
        foreach (var roleClaim in identity.FindAll("roles"))
        {
            if (string.IsNullOrWhiteSpace(roleClaim.Value))
            {
                continue;
            }

            if (!identity.HasClaim(claim => claim.Type == ClaimTypes.Role && claim.Value == roleClaim.Value))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, roleClaim.Value));
            }
        }
    }

    private void EnsureAuthorizedParty(ClaimsIdentity identity)
    {
        var authorizedParty = identity.FindFirst("azp")?.Value;
        if (!string.IsNullOrWhiteSpace(authorizedParty))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_options.SpaClientId))
        {
            return;
        }

        identity.AddClaim(new Claim("azp", _options.SpaClientId));
    }
}
