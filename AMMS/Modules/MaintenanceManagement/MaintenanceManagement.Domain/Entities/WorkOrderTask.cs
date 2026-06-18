using AMMS.Shared.Entities;

namespace MaintenanceManagement.Domain.Entities
{
    public sealed class WorkOrderTask : BaseEntity
    {
        public Guid WorkOrderId { get; private set; }

        public int SequenceNumber { get; private set; }

        public string Description { get; private set; } = null!;

        public bool IsMandatory { get; private set; }
        public bool IsCompleted { get; private set; }

        public Guid? CompletedByUserId { get; private set; }
        public string? CompletedByDisplayName { get; private set; }

        public DateTime? CompletedAt { get; private set; }

        public string? ResultNote { get; private set; }
    }
}
