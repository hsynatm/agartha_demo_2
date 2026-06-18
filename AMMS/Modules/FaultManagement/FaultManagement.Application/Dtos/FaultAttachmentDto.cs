using AMMS.Shared.Dtos;
using FaultManagement.Domain.Enums;

namespace FaultManagement.Application.Dtos
{
    public sealed class FaultAttachmentDto : BaseDto
    {
        public Guid FaultReportId { get; set; }
        public AttachmentType Type { get; set; }
        public string FileName { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public string? ContentType { get; set; }
        public long? FileSize { get; set; }
    }
}
