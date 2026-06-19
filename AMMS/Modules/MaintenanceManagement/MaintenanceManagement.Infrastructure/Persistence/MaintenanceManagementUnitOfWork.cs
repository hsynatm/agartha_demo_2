using AMMS.Infrastructure.Persistence;
using MaintenanceManagement.Domain.Persistence;
using MaintenanceManagement.Domain.Repositories;

namespace MaintenanceManagement.Infrastructure.Persistence;

public sealed class MaintenanceManagementUnitOfWork : EfUnitOfWork<MaintenanceManagementDbContext>, IMaintenanceManagementUnitOfWork
{
    public MaintenanceManagementUnitOfWork(
        MaintenanceManagementDbContext context,
        IMaintenanceManagementRepository workOrders)
        : base(context)
    {
        WorkOrders = workOrders;
    }

    public IMaintenanceManagementRepository WorkOrders { get; }
}
