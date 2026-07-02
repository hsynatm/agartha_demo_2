using AMMS.Shared.Models;
using FaultManagement.Application.Dtos;
using FaultManagement.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AMMS.Api.Controllers;

[Route("api/v1/fault-management/[action]")]
public class FaultManagementController : ApiBaseController
{
    private readonly IFaultManagementService _service;

    public FaultManagementController(IFaultManagementService service)
    {
        _service = service;
    }

    [HttpGet]
    public Task<ActionResult<PagedResult<FaultReportDto>>> GetPaged(
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken) =>
        OkPagedAsync(ct => _service.GetPagedAsync(request, ct), cancellationToken);

    [HttpGet("{id:guid}")]
    public Task<ActionResult<FaultReportDto>> GetById(Guid id, CancellationToken cancellationToken) =>
        OkByIdAsync(_service.GetByIdAsync, id, cancellationToken);

    [HttpPost]
    public Task<ActionResult<FaultReportDto>> Create(
        [FromBody] FaultReportDto request,
        CancellationToken cancellationToken) =>
        CreatedAtGetByIdAsync(request, _service.CreateAsync, result => result.Id, nameof(GetById), cancellationToken);

    [HttpPut("{id:guid}")]
    public Task<ActionResult<FaultReportDto>> Update(
        Guid id,
        [FromBody] FaultReportDto request,
        CancellationToken cancellationToken) =>
        OkValidatedAsync(request, _service.UpdateAsync, id, cancellationToken);

    [HttpDelete("{id:guid}")]
    public Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        NoContentDeleteAsync(_service.DeleteAsync, id, cancellationToken);
}
