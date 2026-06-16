using AMMS.Core.Models;

namespace AMMS.Core.Interfaces.Services
{
    public interface ICurrentUserService
    {
        CurrentUser? CurrentUser { get; }
    }
}
