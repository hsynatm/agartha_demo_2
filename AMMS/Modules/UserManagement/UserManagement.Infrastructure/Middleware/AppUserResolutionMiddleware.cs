using System.Security.Claims;
using AMMS.Infrastructure.Authentication;
using AMMS.Core.Http;
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
        if (!string.IsNullOrWhiteSpace(keycloakUserId))
        {
            var username = context.User.FindFirstValue("preferred_username");
            var appUser = await users.ResolveActiveUserAsync(keycloakUserId, username, context.RequestAborted);

            if (appUser is not null)
            {
                context.Items[HttpContextUserKeys.AppUserId] = appUser.Id;
                context.Items[HttpContextUserKeys.AppUsername] = appUser.Username;

                if (!string.Equals(appUser.KeycloakUserId, keycloakUserId, StringComparison.Ordinal))
                {
                    var trackedUser = await unitOfWork.Users.GetByIdAsync(appUser.Id, context.RequestAborted);
                    if (trackedUser is not null)
                    {
                        trackedUser.KeycloakUserId = keycloakUserId;
                        unitOfWork.Users.Update(trackedUser);
                        await unitOfWork.SaveChangesAsync(context.RequestAborted);
                    }
                }
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
