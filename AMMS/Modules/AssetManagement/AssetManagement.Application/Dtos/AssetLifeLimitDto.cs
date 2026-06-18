using AMMS.Shared.Dtos;
using AssetManagement.Domain.Enums;

namespace AssetManagement.Application.Dtos
{
    public sealed class AssetLifeLimitDto : BaseDto
    {
        public Guid AssetId { get; set; }
        public LifeLimitType Type { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal LimitValue { get; set; }
        public decimal WarningThresholdValue { get; set; }
        public string Unit { get; set; } = null!;
    }
}
