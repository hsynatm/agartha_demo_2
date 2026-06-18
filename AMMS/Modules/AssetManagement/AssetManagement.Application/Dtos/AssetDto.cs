using AMMS.Shared.Dtos;
using AssetManagement.Domain.Enums;

namespace AssetManagement.Application.Dtos
{
    public sealed class AssetDto : BaseDto
    {
        public string AssetCode { get; set; } = null!;
        public string Name { get; set; } = null!;
        public AssetCategory Category { get; set; }
        public AssetStatus Status { get; set; }
        public string? Manufacturer { get; set; }
        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
        public string? TcdsInfo { get; set; }
        public string? TailNumber { get; set; }
        public string? RegistrationCountry { get; set; }
        public string? OperatorName { get; set; }
        public string? CertificationStatus { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public DateTime? CommissioningDate { get; set; }
        public int? ProductionYear { get; set; }
        public int? EconomicLifeYear { get; set; }
        public DateTime? WarrantyEndDate { get; set; }
        public Guid? CurrentLocationId { get; set; }
        public AssetLocationDto? CurrentLocation { get; set; }
        public Guid? AssignedPersonId { get; set; }
        public string? AssignedPersonName { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public ICollection<AssetDocumentDto> Documents { get; set; } = [];
        public ICollection<AssetLocationHistoryDto> LocationHistories { get; set; } = [];
        public ICollection<AssetStatusHistoryDto> StatusHistories { get; set; } = [];
        public ICollection<AssetPartDto> Parts { get; set; } = [];
        public ICollection<AssetLifeLimitDto> LifeLimits { get; set; } = [];
    }
}
