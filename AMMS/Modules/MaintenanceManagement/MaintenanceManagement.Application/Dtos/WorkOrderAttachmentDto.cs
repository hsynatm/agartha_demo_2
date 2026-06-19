using AMMS.Shared.Dtos;
using MaintenanceManagement.Domain.Enums;

namespace MaintenanceManagement.Application.Dtos;

public sealed class WorkOrderAttachmentDto : BaseDto
{
    public Guid WorkOrderId { get; set; }
    public AttachmentType Type { get; set; }
    public string FileName { get; set; } = null!;
    public string FilePath { get; set; } = null!;
    public string? ContentType { get; set; }
}
