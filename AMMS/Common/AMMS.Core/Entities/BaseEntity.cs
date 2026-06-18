using AMMS.Core.Auditing;
using System.ComponentModel.DataAnnotations;

namespace AMMS.Core.Entities
{
    public abstract class BaseEntity
    {
        public Guid Id { get; protected set; } = Guid.NewGuid();

        [Audit.Ignore]
        public DateTime CreatedAt { get; set; }

        [Audit.Ignore]
        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        [Audit.Ignore]
        public DateTime? UpdatedAt { get; set; }

        [Audit.Ignore]
        [MaxLength(100)]
        public string? UpdatedBy { get; set; }

        [Audit.Ignore]
        public bool IsDeleted { get; set; }
    }

}
