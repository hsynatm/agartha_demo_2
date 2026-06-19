using AMMS.Shared.Auditing;
using AMMS.Shared.Entities;
using MaintenanceManagement.Domain.Enums;

namespace MaintenanceManagement.Domain.Entities
{
    public sealed class WorkOrder : BaseEntity, Audit.IAuditableEntity
    {
        public string WorkOrderNumber { get; private set; } = null!;

        // AssetManagement referansı
        public Guid AssetId { get; private set; }

        // Snapshot
        public string AssetCode { get; private set; } = null!;
        public string AssetName { get; private set; } = null!;
        public string? AssetSerialNumber { get; private set; }
        public string? AssetTailNumber { get; private set; }

        // FaultManagement referansı
        public Guid? FaultReportId { get; private set; }
        public string? FaultNumber { get; private set; }

        // MaintenanceManagement referansı
        public Guid? MaintenancePlanId { get; private set; }
        public string? MaintenancePlanCode { get; private set; }

        public WorkOrderType Type { get; private set; }
        public WorkOrderStatus Status { get; private set; }
        public WorkOrderPriority Priority { get; private set; }

        public string Title { get; private set; } = null!;
        public string Description { get; private set; } = null!;

        public DateTime PlannedStartDate { get; private set; }
        public DateTime PlannedEndDate { get; private set; }

        public DateTime? ActualStartDate { get; private set; }
        public DateTime? ActualEndDate { get; private set; }

        public string? HoldReason { get; private set; }
        public string? ClosingNote { get; private set; }

        public ICollection<WorkOrderTask> Tasks { get; private set; } = new List<WorkOrderTask>();
        public ICollection<WorkOrderAssignment> Assignments { get; private set; } = new List<WorkOrderAssignment>();
        public ICollection<WorkOrderMaterial> Materials { get; private set; } = new List<WorkOrderMaterial>();
        public ICollection<WorkOrderTool> Tools { get; private set; } = new List<WorkOrderTool>();
        public ICollection<WorkOrderApproval> Approvals { get; private set; } = new List<WorkOrderApproval>();
        public ICollection<WorkOrderAttachment> Attachments { get; private set; } = new List<WorkOrderAttachment>();
        public ICollection<WorkOrderStatusHistory> StatusHistories { get; private set; } = new List<WorkOrderStatusHistory>();
    }
}
