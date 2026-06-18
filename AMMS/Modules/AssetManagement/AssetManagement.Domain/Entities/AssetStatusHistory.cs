using AMMS.Shared.Entities;
using AssetManagement.Domain.Enums;

namespace AssetManagement.Domain.Entities
{

    public sealed class AssetStatusHistory : BaseEntity
    {
        public Guid AssetId { get; private set; }
        public Asset Asset { get; private set; } = null!;

        public AssetStatus OldStatus { get; private set; }
        public AssetStatus NewStatus { get; private set; }

        public DateTime ChangedAt { get; private set; }

        public string? Reason { get; private set; }
    }
}
