using AMMS.Shared.Models;
using FaultManagement.Application.Dtos;
using FaultManagement.Domain.Enums;

namespace FaultManagement.Application.Services;

public interface IFaultManagementService
{
    Task<FaultReportDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResult<FaultReportDto>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default);

    Task<FaultReportDto> CreateAsync(FaultReportDto request, CancellationToken cancellationToken = default);

    Task<FaultReportDto> UpdateAsync(Guid id, FaultReportDto request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
