using AMMS.Core.Interfaces;
using AMMS.Shared.Models;

namespace AMMS.Infrastructure.Services
{
    public sealed class CurrentOrganizationService : ICurrentOrganizationService
    {
        public CurrentOrganization? CurrentOrganization
        {
            get
            {
                return new CurrentOrganization
                {
                    OrganizationId = "1",
                    Name = "test"
                };
            }
        }
    }

}
