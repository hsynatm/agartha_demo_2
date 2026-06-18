
using AMMS.Shared.Models;

namespace AMMS.Core.Interfaces
{
    public interface ICurrentOrganizationService
    {
        CurrentOrganization? CurrentOrganization { get; }
    }

}
