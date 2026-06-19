using AMMS.Core.Interfaces;
using MaintenanceManagement.Domain.Entities;

namespace MaintenanceManagement.Domain.Repositories;

public interface IMaintenanceManagementRepository : IRepository<WorkOrder>
{
    Task<WorkOrder?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
}
