using AMMS.Shared.Dtos;

namespace AssetManagement.Application.Dtos
{
    public sealed class AssetLocationHistoryDto : BaseDto
    {
        public Guid AssetId { get; set; }
        public Guid LocationId { get; set; }
        public AssetLocationDto? Location { get; set; }
        public DateTime ChangedAt { get; set; }
        public string? Reason { get; set; }
    }
}
