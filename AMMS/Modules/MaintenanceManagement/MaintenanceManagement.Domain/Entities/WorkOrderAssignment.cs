using AMMS.Shared.Entities;
using MaintenanceManagement.Domain.Enums;

namespace MaintenanceManagement.Domain.Entities
{
    public sealed class WorkOrderAssignment : BaseEntity
    {
        public Guid WorkOrderId { get; private set; }

        // User/Personnel service referansı
        public Guid PersonnelId { get; private set; }

        // Snapshot
        public string PersonnelFullName { get; private set; } = null!;
        public string? PersonnelLicenseNumber { get; private set; }

        public AssignmentRole Role { get; private set; }

        public DateTime AssignedAt { get; private set; }

        public Guid AssignedByUserId { get; private set; }
        public string AssignedByDisplayName { get; private set; } = null!;
    }


}
