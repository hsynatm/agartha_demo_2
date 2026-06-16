using System.ComponentModel.DataAnnotations;

namespace AMMS.Core.Entities
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; }

        public DateTime CreatedAt { get; set; }

        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        [MaxLength(100)]
        public string? UpdatedBy { get; set; }

        public bool IsDeleted { get; set; }
    }

}
