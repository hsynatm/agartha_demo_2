using AMMS.Core.Models;

namespace AMMS.Core.Interfaces
{
    public interface ICurrentUserService
    {
        CurrentUser? CurrentUser { get; }
    }

}
