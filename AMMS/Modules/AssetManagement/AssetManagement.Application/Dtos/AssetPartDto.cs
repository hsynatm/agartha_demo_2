using AMMS.Shared.Dtos;

namespace AssetManagement.Application.Dtos
{
    public sealed class AssetPartDto : BaseDto
    {
        public Guid AssetId { get; set; }
        public string PartNumber { get; set; } = null!;
        public string SerialNumber { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Manufacturer { get; set; }
        public DateTime InstalledAt { get; set; }
        public DateTime? RemovedAt { get; set; }
        public string? InstallReason { get; set; }
        public string? RemovalReason { get; set; }
        public Guid? InstalledByPersonnelId { get; set; }
        public string? InstalledByPersonnelName { get; set; }
    }
}
