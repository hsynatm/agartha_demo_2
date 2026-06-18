using AMMS.Shared.Entities;
using MaintenanceManagement.Domain.Enums;

namespace MaintenanceManagement.Domain.Entities
{
    public sealed class MaintenancePlanTrigger : BaseEntity
    {
        public Guid MaintenancePlanId { get; private set; }

        public MaintenanceTriggerType TriggerType { get; private set; }

        public decimal ThresholdValue { get; private set; }
        public string Unit { get; private set; } = null!;

        public decimal? WarningBeforeValue { get; private set; }
    }


}
