using AMMS.Infrastructure.Persistence;
using AMMS.Shared.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AMMS.Infrastructure.Auditing
{
    public abstract class AuditLogEntry
    {
        public Guid Id { get; set; }

        [MaxLength(100)]
        public string? OrganizationId { get; set; }

        [MaxLength(100)]
        public string? UserId { get; set; }

        [MaxLength(100)]
        public string ModuleName { get; set; } = null!;

        [MaxLength(200)]
        public string EntityName { get; set; } = null!;

        [MaxLength(100)]
        public string EntityId { get; set; } = null!;

        public Audit.OperationType OperationType { get; set; }

        [Column(TypeName = "jsonb")]
        public string? OldValues { get; set; }

        [Column(TypeName = "jsonb")]
        public string? NewValues { get; set; }

        [Column(TypeName = "jsonb")]
        public string? ChangedColumns { get; set; }

        [MaxLength(64)]
        public string? IpAddress { get; set; }

        [MaxLength(100)]
        public string? TraceId { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public sealed class AssetManagementAuditLog : AuditLogEntry;

    public sealed class FaultManagementAuditLog : AuditLogEntry;

    public sealed class MaintenanceManagementAuditLog : AuditLogEntry;

    internal sealed record AuditEntry(
        string ModuleName,
        string EntityName,
        string EntityId,
        Audit.OperationType OperationType,
        string? OldValues,
        string? NewValues,
        string? ChangedColumns,
        string? OrganizationId,
        string? UserId,
        string? IpAddress,
        string? TraceId);

    public sealed class AuditDbContext : DbContext
    {
        public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options)
        {
        }

        public DbSet<AssetManagementAuditLog> AssetManagementAuditLogs => Set<AssetManagementAuditLog>();
        public DbSet<FaultManagementAuditLog> FaultManagementAuditLogs => Set<FaultManagementAuditLog>();
        public DbSet<MaintenanceManagementAuditLog> MaintenanceManagementAuditLogs => Set<MaintenanceManagementAuditLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("audit");

            ConfigureAuditTable<AssetManagementAuditLog>(
                modelBuilder,
                AuditModuleNames.GetTableName(AuditModuleNames.AssetManagement));

            ConfigureAuditTable<FaultManagementAuditLog>(
                modelBuilder,
                AuditModuleNames.GetTableName(AuditModuleNames.FaultManagement));

            ConfigureAuditTable<MaintenanceManagementAuditLog>(
                modelBuilder,
                AuditModuleNames.GetTableName(AuditModuleNames.MaintenanceManagement));
        }

        private static void ConfigureAuditTable<TAuditLog>(ModelBuilder modelBuilder, string tableName)
            where TAuditLog : AuditLogEntry
        {
            modelBuilder.Entity<TAuditLog>(entity =>
            {
                entity.ToTable(tableName);
                entity.HasKey(x => x.Id);
                entity.Property(x => x.ModuleName).IsRequired();
                entity.Property(x => x.EntityName).IsRequired();
                entity.Property(x => x.EntityId).IsRequired();
                entity.Property(x => x.OperationType).IsRequired();
                entity.Property(x => x.CreatedAt).IsRequired();
            });
        }
    }

    public sealed class AuditDbContextFactory : IDesignTimeDbContextFactory<AuditDbContext>
    {
        public AuditDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AuditDbContext>();
            optionsBuilder.UseNpgsql(DesignTimeDbConnection.Resolve());
            return new AuditDbContext(optionsBuilder.Options);
        }
    }
}
