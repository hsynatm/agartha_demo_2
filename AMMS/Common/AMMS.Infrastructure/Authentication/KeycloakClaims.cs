using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace AMMS.Infrastructure.Authentication;

public static class KeycloakClaims
{
    public const string PreferredUsernameClaimType = "preferred_username";

    public static string? GetKeycloakUserId(ClaimsPrincipal? principal) => principal?.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal?.FindFirstValue("sub");
}


public sealed class KeycloakClaimsTransformation : IClaimsTransformation
{
    private readonly AmmsAuthenticationOptions _options;

    public KeycloakClaimsTransformation(IOptions<AmmsAuthenticationOptions> options)
    {
        _options = options.Value;
    }

    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
        {
            return Task.FromResult(principal);
        }

        EnsureAuthorizedParty(identity);
        return Task.FromResult(principal);
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
