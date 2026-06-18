using AMMS.Shared.Entities;

namespace FaultManagement.Domain.Entities
{
    public sealed class FaultRepairAction : BaseEntity
    {
        public Guid FaultReportId { get; private set; }

        public string ActionDescription { get; private set; } = null!;

        public DateTime StartedAt { get; private set; }
        public DateTime? CompletedAt { get; private set; }

        public Guid PerformedByUserId { get; private set; }
        public string PerformedByDisplayName { get; private set; } = null!;

        public decimal? LaborHour { get; private set; }
        public decimal? RepairCost { get; private set; }

        public string? ResultDescription { get; private set; }
    }


}
