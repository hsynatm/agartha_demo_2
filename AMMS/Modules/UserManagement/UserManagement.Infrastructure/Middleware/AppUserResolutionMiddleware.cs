using System.Security.Claims;
using AMMS.Core.Http;
using AMMS.Infrastructure.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using UserManagement.Domain.Persistence;
using UserManagement.Domain.Repositories;

namespace UserManagement.Infrastructure.Middleware;

public sealed class AppUserResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public AppUserResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context,IUserManagementRepository users,IUserManagementUnitOfWork unitOfWork)
    {
        var keycloakUserId = KeycloakClaims.GetKeycloakUserId(context.User);
        if (string.IsNullOrWhiteSpace(keycloakUserId))
        {
            await _next(context);
            return;
        }

        var username = context.User.FindFirstValue(KeycloakClaims.PreferredUsernameClaimType);
        var appUser = await users.ResolveActiveUserAsync(keycloakUserId, username, context.RequestAborted);
        if (appUser is null)
        {
            await _next(context);
            return;
        }

        context.Items[HttpContextUserKeys.AppUserId] = appUser.Id;
        context.Items[HttpContextUserKeys.AppUsername] = appUser.Username;

        if (!string.Equals(appUser.KeycloakUserId, keycloakUserId, StringComparison.Ordinal))
        {
            await RepairKeycloakUserIdAsync(unitOfWork, appUser.Id, keycloakUserId, context.RequestAborted);
        }

        await _next(context);
    }

    private static async Task RepairKeycloakUserIdAsync(
        IUserManagementUnitOfWork unitOfWork,
        Guid appUserId,
        string keycloakUserId,
        CancellationToken cancellationToken)
    {
        var trackedUser = await unitOfWork.Users.GetByIdAsync(appUserId, cancellationToken);
        if (trackedUser is null)
        {
            return;
        }

        trackedUser.KeycloakUserId = keycloakUserId;
        unitOfWork.Users.Update(trackedUser);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public static class AppUserResolutionMiddlewareExtensions
{
    public static IApplicationBuilder UseAppUserResolution(this IApplicationBuilder app) =>
        app.UseMiddleware<AppUserResolutionMiddleware>();
}
