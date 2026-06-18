using AMMS.Shared.Entities;
using AssetManagement.Domain.Enums;

namespace AssetManagement.Domain.Entities
{

    public sealed class AssetLocation : BaseEntity
    {
        public string Code { get; private set; } = null!;
        public string Name { get; private set; } = null!;

        public LocationType Type { get; private set; }

        public string? Description { get; private set; }
    }
}
