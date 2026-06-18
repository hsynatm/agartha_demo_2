using AMMS.Shared.Entities;

namespace MaintenanceManagement.Domain.Entities
{
    public sealed class WorkOrderTool : BaseEntity
    {
        public Guid WorkOrderId { get; private set; }

        // Asset/Inventory service referansı
        public Guid ToolId { get; private set; }

        // Snapshot
        public string ToolCode { get; private set; } = null!;
        public string ToolName { get; private set; } = null!;
        public string? CalibrationCertificateNumber { get; private set; }
    }


}
