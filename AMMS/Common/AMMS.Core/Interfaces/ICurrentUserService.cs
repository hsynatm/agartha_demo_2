
using AMMS.Shared.Models;

namespace AMMS.Core.Interfaces
{
    public interface ICurrentUserService
    {
        CurrentUser? CurrentUser { get; }
    }

}
