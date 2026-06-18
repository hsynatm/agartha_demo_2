using AMMS.Shared.Entities;

namespace AssetManagement.Domain.Entities
{
    public sealed class AssetPart : BaseEntity
    {
        public Guid AssetId { get; private set; }
        public Asset Asset { get; private set; } = null!;

        public string PartNumber { get; private set; } = null!;
        public string SerialNumber { get; private set; } = null!;
        public string Name { get; private set; } = null!;

        public string? Manufacturer { get; private set; }

        public DateTime InstalledAt { get; private set; }
        public DateTime? RemovedAt { get; private set; }

        public string? InstallReason { get; private set; }
        public string? RemovalReason { get; private set; }

        public Guid? InstalledByPersonnelId { get; private set; }
        public string? InstalledByPersonnelName { get; private set; }
    }

}
