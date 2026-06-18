using AMMS.Core.Interfaces;
using AMMS.Core.Models;
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

                var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? principal.FindFirstValue("sub");

                if (string.IsNullOrEmpty(userId))
                {
                    return null;
                }

                return new CurrentUser
                {
                    UserId = userId,
                    UserName = principal.Identity?.Name
                        ?? principal.FindFirstValue("preferred_username")
                        ?? string.Empty,
                    Email = principal.FindFirstValue(ClaimTypes.Email),
                    Roles = principal.FindAll(ClaimTypes.Role)
                        .Select(claim => claim.Value)
                        .ToList()
                };
            }
        }
    }



}
