using AMMS.Shared.Dtos;

namespace FaultManagement.Application.Dtos
{
    public sealed class FaultRepairActionDto : BaseDto
    {
        public Guid? FaultReportId { get; set; }
        public string ActionDescription { get; set; } = null!;
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public Guid PerformedByUserId { get; set; }
        public string PerformedByDisplayName { get; set; } = null!;
        public decimal? LaborHour { get; set; }
        public decimal? RepairCost { get; set; }
        public string? ResultDescription { get; set; }
    }
}
