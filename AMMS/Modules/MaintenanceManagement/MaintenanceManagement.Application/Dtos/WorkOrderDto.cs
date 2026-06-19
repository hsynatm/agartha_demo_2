using AMMS.Shared.Dtos;
using MaintenanceManagement.Domain.Enums;

namespace MaintenanceManagement.Application.Dtos;

public sealed class WorkOrderDto : BaseDto
{
    public string WorkOrderNumber { get; set; } = null!;
    public Guid AssetId { get; set; }
    public string AssetCode { get; set; } = null!;
    public string AssetName { get; set; } = null!;
    public string? AssetSerialNumber { get; set; }
    public string? AssetTailNumber { get; set; }
    public Guid? FaultReportId { get; set; }
    public string? FaultNumber { get; set; }
    public Guid? MaintenancePlanId { get; set; }
    public string? MaintenancePlanCode { get; set; }
    public WorkOrderType Type { get; set; }
    public WorkOrderStatus Status { get; set; }
    public WorkOrderPriority Priority { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime PlannedStartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public string? HoldReason { get; set; }
    public string? ClosingNote { get; set; }
    public ICollection<WorkOrderTaskDto> Tasks { get; set; } = [];
    public ICollection<WorkOrderAssignmentDto> Assignments { get; set; } = [];
    public ICollection<WorkOrderMaterialDto> Materials { get; set; } = [];
    public ICollection<WorkOrderToolDto> Tools { get; set; } = [];
    public ICollection<WorkOrderApprovalDto> Approvals { get; set; } = [];
    public ICollection<WorkOrderAttachmentDto> Attachments { get; set; } = [];
    public ICollection<WorkOrderStatusHistoryDto> StatusHistories { get; set; } = [];
}
