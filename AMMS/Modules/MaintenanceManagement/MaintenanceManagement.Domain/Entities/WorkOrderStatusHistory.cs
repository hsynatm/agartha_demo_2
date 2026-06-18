using AMMS.Shared.Entities;
using MaintenanceManagement.Domain.Enums;

namespace MaintenanceManagement.Domain.Entities
{
    public sealed class WorkOrderStatusHistory : BaseEntity
    {
        public Guid WorkOrderId { get; private set; }

        public WorkOrderStatus OldStatus { get; private set; }
        public WorkOrderStatus NewStatus { get; private set; }

        public string? Reason { get; private set; }

        public Guid ChangedByUserId { get; private set; }
        public string ChangedByDisplayName { get; private set; } = null!;

        public DateTime ChangedAt { get; private set; }
    }


}
