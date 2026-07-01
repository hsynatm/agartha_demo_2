using AMMS.Core.Interfaces;
using AMMS.Infrastructure.Authentication;
using AMMS.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace AMMS.Infrastructure.Services
{
    public sealed class CurrentOrganizationService : ICurrentOrganizationService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AmmsAuthenticationOptions _options;

        public CurrentOrganizationService(IHttpContextAccessor httpContextAccessor,IOptions<AmmsAuthenticationOptions> options)
        {
            _httpContextAccessor = httpContextAccessor;
            _options = options.Value;
        }

        public CurrentOrganization? CurrentOrganization
        {
            get
            {
                var principal = _httpContextAccessor.HttpContext?.User;
                if (principal is null or { Identity.IsAuthenticated: false })
                {
                    return null;
                }

                var organizationId = principal.FindFirstValue(_options.OrganizationClaimType);
                if (string.IsNullOrWhiteSpace(organizationId))
                {
                    return null;
                }

                var organizationName = principal.FindFirstValue(_options.OrganizationNameClaimType)
                    ?? organizationId;

                return new CurrentOrganization
                {
                    OrganizationId = organizationId,
                    Name = organizationName
                };
            }
        }
    }
}
