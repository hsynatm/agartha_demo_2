using AMMS.Infrastructure.Persistence;
using AMMS.Shared.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AMMS.Infrastructure.Auditing
{


    [Table("audit_logs")]
    public class AuditLog
    {
        public Guid Id { get; set; }

        [MaxLength(100)]
        public string? OrganizationId { get; set; }

        [MaxLength(100)]
        public string? UserId { get; set; }

        [MaxLength(100)]
        public required string ModuleName { get; set; }

        [MaxLength(200)]
        public required string EntityName { get; set; }

        [MaxLength(100)]
        public required string EntityId { get; set; }

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

        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("audit");

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.ToTable("audit_logs");
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
