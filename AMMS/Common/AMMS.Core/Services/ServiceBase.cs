using AMMS.Core.Interfaces.Persistence;

namespace AMMS.Core.Services
{
    public abstract class ServiceBase
    {
        protected IUnitOfWork UnitOfWork { get; }

        protected ServiceBase(IUnitOfWork unitOfWork)
        {
            UnitOfWork = unitOfWork;
        }

        protected Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            UnitOfWork.SaveChangesAsync(cancellationToken);
    }

}
