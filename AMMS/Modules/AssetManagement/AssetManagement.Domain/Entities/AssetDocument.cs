using AMMS.Shared.Auditing;
using AMMS.Shared.Entities;
using AssetManagement.Domain.Enums;

namespace AssetManagement.Domain.Entities
{

    public sealed class AssetDocument : BaseEntity, Audit.IAuditableEntity
    {
        public Guid AssetId { get; private set; }
        public Asset Asset { get; private set; } = null!;

        public AssetDocumentType DocumentType { get; private set; }

        public string DocumentNumber { get; private set; } = null!;
        public string FileName { get; private set; } = null!;
        public string FilePath { get; private set; } = null!;

        public DateTime? IssueDate { get; private set; }
        public DateTime? ExpiryDate { get; private set; }

        public int Version { get; private set; }
    }

}
