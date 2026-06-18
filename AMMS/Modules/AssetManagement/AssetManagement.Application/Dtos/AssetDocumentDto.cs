using AMMS.Shared.Dtos;
using AssetManagement.Domain.Enums;

namespace AssetManagement.Application.Dtos
{
    public sealed class AssetDocumentDto : BaseDto
    {
        public Guid AssetId { get; set; }
        public AssetDocumentType DocumentType { get; set; }
        public string DocumentNumber { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int Version { get; set; }
    }
}
