using AMMS.Shared.Entities;
using MaintenanceManagement.Domain.Enums;

namespace MaintenanceManagement.Domain.Entities
{
    public sealed class WorkOrderAttachment : BaseEntity
    {
        public Guid WorkOrderId { get; private set; }
        public AttachmentType Type { get; private set; }
        public string FileName { get; private set; } = null!;
        public string FilePath { get; private set; } = null!;
        public string? ContentType { get; private set; }
    }
}
