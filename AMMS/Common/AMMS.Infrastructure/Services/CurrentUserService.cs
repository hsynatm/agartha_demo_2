using AMMS.Core.Http;
using AMMS.Core.Interfaces;
using AMMS.Infrastructure.Authentication;
using AMMS.Shared.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace AMMS.Infrastructure.Services
{
    public sealed class CurrentUserService : ICurrentUserService
    {
      
        public static CurrentUserService Empty { get; } = new();

        private readonly IHttpContextAccessor? _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private CurrentUserService()
        {
        }

        public CurrentUser? CurrentUser
        {
            get
            {
                var principal = _httpContextAccessor?.HttpContext?.User;
                if (principal is null or { Identity.IsAuthenticated: false })
                {
                    return null;
                }

                var keycloakUserId = KeycloakClaims.GetKeycloakUserId(principal);

                if (string.IsNullOrEmpty(keycloakUserId))
                {
                    return null;
                }

                var httpContext = _httpContextAccessor?.HttpContext;
                var appUserId = httpContext?.Items[HttpContextUserKeys.AppUserId] as Guid?;
                var appUsername = httpContext?.Items[HttpContextUserKeys.AppUsername] as string;

                return new CurrentUser
                {
                    UserId = appUserId?.ToString() ?? keycloakUserId,
                    KeycloakUserId = keycloakUserId,
                    UserName = appUsername
                        ?? principal.Identity?.Name
                        ?? principal.FindFirstValue("preferred_username")
                        ?? string.Empty,
                    Email = principal.FindFirstValue(ClaimTypes.Email),
                    Roles = []
                };
            }
        }
    }



}
