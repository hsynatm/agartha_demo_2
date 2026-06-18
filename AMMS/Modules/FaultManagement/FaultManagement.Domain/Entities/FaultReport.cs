using AMMS.Shared.Entities;
using FaultManagement.Domain.Enums;

namespace FaultManagement.Domain.Entities
{
    public sealed class FaultReport : BaseEntity
    {
        public string FaultNumber { get; private set; } = null!;

        // AssetManagement referansı
        public Guid AssetId { get; private set; }

        // Snapshot bilgiler
        public string AssetCode { get; private set; } = null!;
        public string AssetName { get; private set; } = null!;
        public string? AssetSerialNumber { get; private set; }
        public string? AssetTailNumber { get; private set; }

        public string Title { get; private set; } = null!;
        public string Description { get; private set; } = null!;

        public FaultType Type { get; private set; }
        public FaultPriority Priority { get; private set; }
        public FaultImpactType ImpactType { get; private set; }
        public FaultStatus Status { get; private set; }

        public int? RpnScore { get; private set; }

        public DateTime ReportedAt { get; private set; }
        public Guid ReportedByUserId { get; private set; }
        public string ReportedByDisplayName { get; private set; } = null!;

        public Guid? AssignedToUserId { get; private set; }
        public string? AssignedToDisplayName { get; private set; }

        public decimal? Latitude { get; private set; }
        public decimal? Longitude { get; private set; }

        public ICollection<FaultAttachment> Attachments { get; private set; } = new List<FaultAttachment>();
        public ICollection<FaultActivity> Activities { get; private set; } = new List<FaultActivity>();
        public ICollection<FaultRepairAction> RepairActions { get; private set; } = new List<FaultRepairAction>();
    }
}
