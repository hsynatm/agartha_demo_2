using AMMS.Shared.Dtos;
using MaintenanceManagement.Domain.Enums;

namespace MaintenanceManagement.Application.Dtos;

public sealed class WorkOrderStatusHistoryDto : BaseDto
{
    public Guid WorkOrderId { get; set; }
    public WorkOrderStatus OldStatus { get; set; }
    public WorkOrderStatus NewStatus { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? Reason { get; set; }
}
