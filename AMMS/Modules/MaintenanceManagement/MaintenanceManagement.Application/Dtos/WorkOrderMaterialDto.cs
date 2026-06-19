using AMMS.Shared.Dtos;

namespace MaintenanceManagement.Application.Dtos;

public sealed class WorkOrderMaterialDto : BaseDto
{
    public Guid WorkOrderId { get; set; }
    public Guid InventoryItemId { get; set; }
    public string MaterialCode { get; set; } = null!;
    public string MaterialName { get; set; } = null!;
    public string? SerialNumber { get; set; }
    public string? LotNumber { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = null!;
}
