using AMMS.Shared.Entities;

namespace MaintenanceManagement.Domain.Entities
{
    public sealed class WorkOrderMaterial : BaseEntity
    {
        public Guid WorkOrderId { get; private set; }

        // Inventory service referansı
        public Guid InventoryItemId { get; private set; }

        // Snapshot
        public string MaterialCode { get; private set; } = null!;
        public string MaterialName { get; private set; } = null!;
        public string? SerialNumber { get; private set; }
        public string? LotNumber { get; private set; }

        public decimal Quantity { get; private set; }

        public string Unit { get; private set; } = null!;
    }

}
