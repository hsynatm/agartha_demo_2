using AMMS.Shared.Models;
using MaintenanceManagement.Application.Dtos;
using MaintenanceManagement.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AMMS.Api.Controllers;

[Route("api/v1/maintenance-management/[action]")]
public class MaintenanceManagementController : ApiBaseController
{
    private readonly IMaintenanceManagementService _service;

    public MaintenanceManagementController(IMaintenanceManagementService service)
    {
        _service = service;
    }

    [HttpGet]
    public Task<ActionResult<PagedResult<WorkOrderDto>>> GetPaged(
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken) =>
        OkPagedAsync(ct => _service.GetPagedAsync(request, ct), cancellationToken);

    [HttpGet("{id:guid}")]
    public Task<ActionResult<WorkOrderDto>> GetById(Guid id, CancellationToken cancellationToken) =>
        OkByIdAsync(_service.GetByIdAsync, id, cancellationToken);

    [HttpPost]
    public Task<ActionResult<WorkOrderDto>> Create(
        [FromBody] WorkOrderDto request,
        CancellationToken cancellationToken) =>
        CreatedAtGetByIdAsync(request, _service.CreateAsync, result => result.Id, nameof(GetById), cancellationToken);

    [HttpPut("{id:guid}")]
    public Task<ActionResult<WorkOrderDto>> Update(
        Guid id,
        [FromBody] WorkOrderDto request,
        CancellationToken cancellationToken) =>
        OkValidatedAsync(request, _service.UpdateAsync, id, cancellationToken);

    [HttpDelete("{id:guid}")]
    public Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        NoContentDeleteAsync(_service.DeleteAsync, id, cancellationToken);
}
