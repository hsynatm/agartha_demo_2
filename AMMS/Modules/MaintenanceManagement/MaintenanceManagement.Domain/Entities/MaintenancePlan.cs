using AMMS.Shared.Entities;
using MaintenanceManagement.Domain.Enums;

namespace MaintenanceManagement.Domain.Entities
{

    public sealed class MaintenancePlan : BaseEntity
    {
        public string PlanCode { get; private set; } = null!;
        public string Name { get; private set; } = null!;
        public string? Description { get; private set; }

        // AssetManagement referansı
        public Guid AssetId { get; private set; }

        // Snapshot
        public string AssetCode { get; private set; } = null!;
        public string AssetName { get; private set; } = null!;

        public MaintenanceType Type { get; private set; }

        public bool IsActive { get; private set; }

        public DateTime? ValidFrom { get; private set; }
        public DateTime? ValidTo { get; private set; }

        public ICollection<MaintenancePlanTask> Tasks { get; private set; } = new List<MaintenancePlanTask>();
        public ICollection<MaintenancePlanTrigger> Triggers { get; private set; } = new List<MaintenancePlanTrigger>();
    }


}
