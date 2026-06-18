using AMMS.Shared.Entities;

namespace AssetManagement.Domain.Entities
{
    public sealed class AssetLocationHistory : BaseEntity
    {
        public Guid AssetId { get; private set; }
        public Asset Asset { get; private set; } = null!;

        public Guid LocationId { get; private set; }
        public AssetLocation Location { get; private set; } = null!;

        public DateTime ChangedAt { get; private set; }

        public string? Reason { get; private set; }
    }

}
