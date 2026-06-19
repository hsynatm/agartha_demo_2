using AMMS.Core.Interfaces;
using AMMS.Infrastructure.Persistence;
using FaultManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FaultManagement.Infrastructure.Persistence;

public class FaultManagementDbContext : BaseDbContext
{
    public FaultManagementDbContext(DbContextOptions<FaultManagementDbContext> options, ICurrentUserService currentUserService)
        : base(options, currentUserService)
    {
    }

    public DbSet<FaultReport> FaultReports => Set<FaultReport>();
    public DbSet<FaultActivity> FaultActivities => Set<FaultActivity>();
    public DbSet<FaultAttachment> FaultAttachments => Set<FaultAttachment>();
    public DbSet<FaultRepairAction> FaultRepairActions => Set<FaultRepairAction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(DependencyInjection.SchemaName);
        base.OnModelCreating(modelBuilder);
    }
}
