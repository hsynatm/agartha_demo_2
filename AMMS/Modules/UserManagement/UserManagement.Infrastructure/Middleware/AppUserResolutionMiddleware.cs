using AMMS.Infrastructure.Authentication;
using AMMS.Core.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using UserManagement.Domain.Repositories;

namespace UserManagement.Infrastructure.Middleware;

public sealed class AppUserResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public AppUserResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IUserManagementRepository users)
    {
        var keycloakUserId = KeycloakClaims.GetKeycloakUserId(context.User);
        if (!string.IsNullOrWhiteSpace(keycloakUserId))
        {
            var appUser = await users.GetByKeycloakUserIdAsync(keycloakUserId, context.RequestAborted);
            if (appUser is not null)
            {
                context.Items[HttpContextUserKeys.AppUserId] = appUser.Id;
                context.Items[HttpContextUserKeys.AppUsername] = appUser.Username;
            }
        }

        await _next(context);
    }
}

public static class AppUserResolutionMiddlewareExtensions
{
    public static IApplicationBuilder UseAppUserResolution(this IApplicationBuilder app) =>
        app.UseMiddleware<AppUserResolutionMiddleware>();
}
