using AMMS.Shared.Dtos;

namespace MaintenanceManagement.Application.Dtos;

public sealed class WorkOrderToolDto : BaseDto
{
    public Guid WorkOrderId { get; set; }
    public Guid ToolId { get; set; }
    public string ToolCode { get; set; } = null!;
    public string ToolName { get; set; } = null!;
    public string? CalibrationCertificateNumber { get; set; }
}
