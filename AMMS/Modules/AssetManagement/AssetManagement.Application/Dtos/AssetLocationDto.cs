using AMMS.Shared.Dtos;
using AssetManagement.Domain.Enums;

namespace AssetManagement.Application.Dtos
{
    public sealed class AssetLocationDto : BaseDto
    {
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public LocationType Type { get; set; }
        public string? Description { get; set; }
    }
}
