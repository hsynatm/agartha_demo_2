using AMMS.Shared.Entities;
using FaultManagement.Domain.Enums;

namespace FaultManagement.Domain.Entities
{
    public sealed class FaultAttachment : BaseEntity
    {
        public Guid FaultReportId { get; private set; }

        public AttachmentType Type { get; private set; }

        public string FileName { get; private set; } = null!;
        public string FilePath { get; private set; } = null!;
        public string? ContentType { get; private set; }
        public long? FileSize { get; private set; }
    }


}
