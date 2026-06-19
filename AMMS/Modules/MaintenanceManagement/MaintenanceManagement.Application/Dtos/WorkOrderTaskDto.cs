using AMMS.Shared.Dtos;

namespace MaintenanceManagement.Application.Dtos;

public sealed class WorkOrderTaskDto : BaseDto
{
    public Guid WorkOrderId { get; set; }
    public int SequenceNumber { get; set; }
    public string Description { get; set; } = null!;
    public bool IsMandatory { get; set; }
    public bool IsCompleted { get; set; }
    public Guid? CompletedByUserId { get; set; }
    public string? CompletedByDisplayName { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ResultNote { get; set; }
}
