using AMMS.Shared.Models;
using MaintenanceManagement.Application.Dtos;

namespace MaintenanceManagement.Application.Services;

public interface IMaintenanceManagementService
{
    Task<WorkOrderDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResult<WorkOrderDto>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default);

    Task<WorkOrderDto> CreateAsync(WorkOrderDto request, CancellationToken cancellationToken = default);

    Task<WorkOrderDto> UpdateAsync(Guid id, WorkOrderDto request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
