using AMMS.Shared.Auditing;
using AMMS.Shared.Entities;
using AssetManagement.Domain.Enums;

namespace AssetManagement.Domain.Entities
{

    public sealed class Asset : BaseEntity, Audit.IAuditableEntity
    {
        public string AssetCode { get; private set; } = null!;
        public string Name { get; private set; } = null!;

        public AssetCategory Category { get; private set; }
        public AssetStatus Status { get; private set; }

        public string? Manufacturer { get; private set; }
        public string? Model { get; private set; }
        public string? SerialNumber { get; private set; }
        public string? TcdsInfo { get; private set; }

        // Aircraft-specific
        public string? TailNumber { get; private set; }
        public string? RegistrationCountry { get; private set; }
        public string? OperatorName { get; private set; }
        public string? CertificationStatus { get; private set; }

        public DateTime? PurchaseDate { get; private set; }
        public DateTime? CommissioningDate { get; private set; }
        public int? ProductionYear { get; private set; }
        public int? EconomicLifeYear { get; private set; }
        public DateTime? WarrantyEndDate { get; private set; }

        public Guid? CurrentLocationId { get; private set; }
        public AssetLocation? CurrentLocation { get; private set; }

        public Guid? AssignedPersonId { get; private set; }
        public string? AssignedPersonName { get; private set; }

        public Guid? DepartmentId { get; private set; }
        public string? DepartmentName { get; private set; }

        public ICollection<AssetDocument> Documents { get; private set; } = new List<AssetDocument>();
        public ICollection<AssetLocationHistory> LocationHistories { get; private set; } = new List<AssetLocationHistory>();
        public ICollection<AssetStatusHistory> StatusHistories { get; private set; } = new List<AssetStatusHistory>();
        public ICollection<AssetPart> Parts { get; private set; } = new List<AssetPart>();
        public ICollection<AssetLifeLimit> LifeLimits { get; private set; } = new List<AssetLifeLimit>();
    }
}
