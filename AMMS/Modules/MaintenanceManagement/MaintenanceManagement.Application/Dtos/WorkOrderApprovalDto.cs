using AMMS.Shared.Dtos;
using MaintenanceManagement.Domain.Enums;

namespace MaintenanceManagement.Application.Dtos;

public sealed class WorkOrderApprovalDto : BaseDto
{
    public Guid WorkOrderId { get; set; }
    public ApprovalStepType StepType { get; set; }
    public Guid ApproverUserId { get; set; }
    public string ApproverDisplayName { get; set; } = null!;
    public ApprovalStatus Status { get; set; }
    public string? Comment { get; set; }
    public DateTime? ApprovedAt { get; set; }
}
