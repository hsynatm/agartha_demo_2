using AMMS.Core.Interfaces;
using MaintenanceManagement.Domain.Repositories;

namespace MaintenanceManagement.Domain.Persistence;

public interface IMaintenanceManagementUnitOfWork : IUnitOfWork
{
    IMaintenanceManagementRepository WorkOrders { get; }
}
