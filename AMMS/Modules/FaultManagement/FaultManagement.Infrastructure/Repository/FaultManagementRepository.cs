using AMMS.Infrastructure.Persistence;
using FaultManagement.Domain.Entities;
using FaultManagement.Domain.Repositories;
using FaultManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FaultManagement.Infrastructure.Repository;

public class FaultManagementRepository : EfRepository<FaultReport>, IFaultManagementRepository
{
    private readonly FaultManagementDbContext _context;

    public FaultManagementRepository(FaultManagementDbContext context)
        : base(context)
    {
        _context = context;
    }

    public async Task<FaultReport?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.FaultReports
            .AsNoTracking()
            .Include(report => report.Attachments)
            .Include(report => report.Activities)
            .Include(report => report.RepairActions)
            .FirstOrDefaultAsync(report => report.Id == id, cancellationToken);
}
