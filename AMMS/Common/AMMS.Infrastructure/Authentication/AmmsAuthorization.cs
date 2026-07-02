using System.Security.Claims;
using AMMS.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace AMMS.Infrastructure.Authentication;

/// <summary>
/// PostgreSQL-backed authorization. Roles and role groups are evaluated from the database, not JWT claims.
/// When both are specified, the user needs any matching role or any matching role group (OR).
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class AmmsAuthorizeAttribute : AuthorizeAttribute, IAuthorizationRequirementData
{
    public string? RequiredRoles { get; set; }

    public string? RoleGroups { get; set; }

    public IEnumerable<IAuthorizationRequirement> GetRequirements()
    {
        yield return new AmmsPermissionRequirement(ParseList(RequiredRoles), ParseList(RoleGroups));
    }

    private static IReadOnlyCollection<string> ParseList(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        return value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}

public sealed class AmmsPermissionRequirement : IAuthorizationRequirement
{
    public AmmsPermissionRequirement(
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<string> roleGroups)
    {
        Roles = roles;
        RoleGroups = roleGroups;
    }

    public IReadOnlyCollection<string> Roles { get; }

    public IReadOnlyCollection<string> RoleGroups { get; }
}

public sealed class AmmsPermissionAuthorizationHandler : AuthorizationHandler<AmmsPermissionRequirement>
{
    private readonly IUserAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AmmsPermissionAuthorizationHandler(
        IUserAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor)
    {
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AmmsPermissionRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        if (requirement.Roles.Count == 0 && requirement.RoleGroups.Count == 0)
        {
            context.Succeed(requirement);
            return;
        }

        var keycloakUserId = KeycloakClaims.GetKeycloakUserId(context.User);
        if (string.IsNullOrWhiteSpace(keycloakUserId))
        {
            SetFailureContext(
                "Keycloak user id is missing from the access token.",
                requiredRoles: requirement.Roles,
                requiredRoleGroups: requirement.RoleGroups);
            return;
        }

        var username = context.User.FindFirstValue(KeycloakClaims.PreferredUsernameClaimType);
        var authorized = await _authorizationService.IsAuthorizedAsync(
            keycloakUserId,
            requirement.Roles,
            requirement.RoleGroups,
            username,
            _httpContextAccessor.HttpContext?.RequestAborted ?? CancellationToken.None);

        if (authorized)
        {
            context.Succeed(requirement);
            return;
        }

        SetFailureContext(
            "User does not have the required roles or role groups.",
            keycloakUserId,
            requirement.Roles,
            requirement.RoleGroups);
    }

    private void SetFailureContext(string reason,string? keycloakUserId = null,IReadOnlyCollection<string>? requiredRoles = null,IReadOnlyCollection<string>? requiredRoleGroups = null)
    {
        AuthorizationFailureContext.Set(
            _httpContextAccessor.HttpContext,
            new AuthorizationFailureSnapshot(
                reason,
                keycloakUserId,
                JoinCodes(requiredRoles),
                JoinCodes(requiredRoleGroups)));
    }

    private static string? JoinCodes(IReadOnlyCollection<string>? codes) => codes is null or { Count: 0 } ? null : string.Join(", ", codes);
}
