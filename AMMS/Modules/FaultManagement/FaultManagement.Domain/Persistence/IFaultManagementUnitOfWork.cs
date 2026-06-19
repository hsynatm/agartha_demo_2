using AMMS.Core.Interfaces;
using FaultManagement.Domain.Repositories;

namespace FaultManagement.Domain.Persistence;

public interface IFaultManagementUnitOfWork : IUnitOfWork
{
    IFaultManagementRepository FaultReports { get; }
}
