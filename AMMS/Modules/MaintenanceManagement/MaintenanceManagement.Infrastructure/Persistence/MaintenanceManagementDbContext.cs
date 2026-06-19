using AMMS.Core.Interfaces;
using AMMS.Infrastructure.Persistence;
using MaintenanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MaintenanceManagement.Infrastructure.Persistence;

public class MaintenanceManagementDbContext : BaseDbContext
{
    public MaintenanceManagementDbContext(DbContextOptions<MaintenanceManagementDbContext> options, ICurrentUserService currentUserService)
        : base(options, currentUserService)
    {
    }

    public DbSet<MaintenancePlan> MaintenancePlans => Set<MaintenancePlan>();
    public DbSet<MaintenancePlanTask> MaintenancePlanTasks => Set<MaintenancePlanTask>();
    public DbSet<MaintenancePlanTrigger> MaintenancePlanTriggers => Set<MaintenancePlanTrigger>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<WorkOrderApproval> WorkOrderApprovals => Set<WorkOrderApproval>();
    public DbSet<WorkOrderAssignment> WorkOrderAssignments => Set<WorkOrderAssignment>();
    public DbSet<WorkOrderAttachment> WorkOrderAttachments => Set<WorkOrderAttachment>();
    public DbSet<WorkOrderMaterial> WorkOrderMaterials => Set<WorkOrderMaterial>();
    public DbSet<WorkOrderStatusHistory> WorkOrderStatusHistories => Set<WorkOrderStatusHistory>();
    public DbSet<WorkOrderTask> WorkOrderTasks => Set<WorkOrderTask>();
    public DbSet<WorkOrderTool> WorkOrderTools => Set<WorkOrderTool>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(DependencyInjection.SchemaName);
        base.OnModelCreating(modelBuilder);
    }
}
