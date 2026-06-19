using AMMS.Shared.Dtos;
using FaultManagement.Domain.Enums;

namespace FaultManagement.Application.Dtos
{
    public sealed class FaultActivityDto : BaseDto
    {
        public Guid? FaultReportId { get; set; }
        public FaultActivityType ActivityType { get; set; }
        public string Description { get; set; } = null!;
        public Guid PerformedByUserId { get; set; }
        public string PerformedByDisplayName { get; set; } = null!;
        public DateTime PerformedAt { get; set; }
    }
}
