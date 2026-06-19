using AMMS.Shared.Dtos;
using MaintenanceManagement.Domain.Enums;

namespace MaintenanceManagement.Application.Dtos;

public sealed class WorkOrderAssignmentDto : BaseDto
{
    public Guid WorkOrderId { get; set; }
    public Guid PersonnelId { get; set; }
    public string PersonnelFullName { get; set; } = null!;
    public string? PersonnelLicenseNumber { get; set; }
    public AssignmentRole Role { get; set; }
    public DateTime AssignedAt { get; set; }
    public Guid AssignedByUserId { get; set; }
    public string AssignedByDisplayName { get; set; } = null!;
}
