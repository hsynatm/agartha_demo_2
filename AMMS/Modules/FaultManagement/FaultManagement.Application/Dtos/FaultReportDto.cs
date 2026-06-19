using AMMS.Shared.Dtos;
using FaultManagement.Domain.Enums;

namespace FaultManagement.Application.Dtos
{
    public sealed class FaultReportDto : BaseDto
    {
        public string FaultNumber { get; set; } = null!;
        public Guid AssetId { get; set; }
        public string AssetCode { get; set; } = null!;
        public string AssetName { get; set; } = null!;
        public string? AssetSerialNumber { get; set; }
        public string? AssetTailNumber { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public FaultType Type { get; set; }
        public FaultPriority Priority { get; set; }
        public FaultImpactType ImpactType { get; set; }
        public FaultStatus Status { get; set; }
        public int? RpnScore { get; set; }
        public DateTime? ReportedAt { get; set; }
        public Guid? ReportedByUserId { get; set; }
        public string ReportedByDisplayName { get; set; } = null!;
        public Guid? AssignedToUserId { get; set; }
        public string? AssignedToDisplayName { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public ICollection<FaultAttachmentDto>? Attachments { get; set; } = [];
        public ICollection<FaultActivityDto>? Activities { get; set; } = [];
        public ICollection<FaultRepairActionDto>? RepairActions { get; set; } = [];
    }
}
