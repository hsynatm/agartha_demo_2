using AMMS.Core.Interfaces;
using FaultManagement.Domain.Entities;

namespace FaultManagement.Domain.Repositories;

public interface IFaultManagementRepository : IRepository<FaultReport>
{
    Task<FaultReport?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
}
