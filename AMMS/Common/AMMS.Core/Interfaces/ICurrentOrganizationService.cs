using AMMS.Core.Models;

namespace AMMS.Core.Interfaces
{
    public interface ICurrentOrganizationService
    {
        CurrentOrganization? CurrentOrganization { get; }
    }

}
