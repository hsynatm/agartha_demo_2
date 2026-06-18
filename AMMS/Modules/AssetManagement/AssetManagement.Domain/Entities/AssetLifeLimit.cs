using AMMS.Shared.Entities;
using AssetManagement.Domain.Enums;

namespace AssetManagement.Domain.Entities
{

    public sealed class AssetLifeLimit : BaseEntity
    {
        public Guid AssetId { get; private set; }
        public Asset Asset { get; private set; } = null!;

        public LifeLimitType Type { get; private set; }

        public decimal CurrentValue { get; private set; }
        public decimal LimitValue { get; private set; }
        public decimal WarningThresholdValue { get; private set; }

        public string Unit { get; private set; } = null!;
    }
}
