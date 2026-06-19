using AMMS.Shared.Auditing;
using AMMS.Shared.Entities;

namespace MaintenanceManagement.Domain.Entities
{

    public sealed class MaintenancePlanTask : BaseEntity, Audit.IAuditableEntity
    {
        public Guid MaintenancePlanId { get; private set; }

        public string TaskCode { get; private set; } = null!;
        public string Description { get; private set; } = null!;

        public int SequenceNumber { get; private set; }

        public bool IsMandatory { get; private set; }

        public string? ReferenceDocumentCode { get; private set; } // AMM, CMM, SB, AD vb.
        public string? SafetyWarning { get; private set; }
    }


}
