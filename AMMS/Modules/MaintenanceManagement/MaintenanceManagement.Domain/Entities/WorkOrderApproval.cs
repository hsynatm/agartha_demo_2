using AMMS.Shared.Entities;
using MaintenanceManagement.Domain.Enums;

namespace MaintenanceManagement.Domain.Entities
{
    public sealed class WorkOrderApproval : BaseEntity
    {
        public Guid WorkOrderId { get; private set; }

        public ApprovalStepType StepType { get; private set; }

        public Guid ApproverUserId { get; private set; }
        public string ApproverDisplayName { get; private set; } = null!;

        public ApprovalStatus Status { get; private set; }

        public string? Comment { get; private set; }

        public DateTime? ApprovedAt { get; private set; }
    }
}
