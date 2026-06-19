using AMMS.Infrastructure.Persistence;
using MaintenanceManagement.Domain.Entities;
using MaintenanceManagement.Domain.Repositories;
using MaintenanceManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MaintenanceManagement.Infrastructure.Repository;

public class MaintenanceManagementRepository : EfRepository<WorkOrder>, IMaintenanceManagementRepository
{
    private readonly MaintenanceManagementDbContext _context;

    public MaintenanceManagementRepository(MaintenanceManagementDbContext context)
        : base(context)
    {
        _context = context;
    }

    public async Task<WorkOrder?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.WorkOrders
            .AsNoTracking()
            .Include(order => order.Tasks)
            .Include(order => order.Assignments)
            .Include(order => order.Materials)
            .Include(order => order.Tools)
            .Include(order => order.Approvals)
            .Include(order => order.Attachments)
            .Include(order => order.StatusHistories)
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);
}
