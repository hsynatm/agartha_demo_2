using AMMS.Shared.Dtos;
using AssetManagement.Domain.Enums;

namespace AssetManagement.Application.Dtos
{
    public sealed class AssetStatusHistoryDto : BaseDto
    {
        public Guid AssetId { get; set; }
        public AssetStatus OldStatus { get; set; }
        public AssetStatus NewStatus { get; set; }
        public DateTime ChangedAt { get; set; }
        public string? Reason { get; set; }
    }
}
