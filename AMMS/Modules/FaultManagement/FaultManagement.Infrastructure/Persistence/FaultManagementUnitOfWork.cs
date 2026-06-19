using AMMS.Infrastructure.Persistence;
using FaultManagement.Domain.Persistence;
using FaultManagement.Domain.Repositories;

namespace FaultManagement.Infrastructure.Persistence;

public sealed class FaultManagementUnitOfWork : EfUnitOfWork<FaultManagementDbContext>, IFaultManagementUnitOfWork
{
    public FaultManagementUnitOfWork(
        FaultManagementDbContext context,
        IFaultManagementRepository faultReports)
        : base(context)
    {
        FaultReports = faultReports;
    }

    public IFaultManagementRepository FaultReports { get; }
}
