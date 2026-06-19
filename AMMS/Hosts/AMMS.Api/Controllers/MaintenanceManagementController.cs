using AMMS.Shared.Models;
using MaintenanceManagement.Application.Dtos;
using MaintenanceManagement.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AMMS.Api.Controllers;

[Route("api/v1/maintenance-management")]
public class MaintenanceManagementController : ApiBaseController
{
    private readonly IMaintenanceManagementService _service;

    public MaintenanceManagementController(IMaintenanceManagementService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<WorkOrderDto>>> GetPaged(
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetPagedAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WorkOrderDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<WorkOrderDto>> Create(
        [FromBody] WorkOrderDto request,
        CancellationToken cancellationToken)
    {
        EnsureValidRequest();

        var result = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<WorkOrderDto>> Update(
        Guid id,
        [FromBody] WorkOrderDto request,
        CancellationToken cancellationToken)
    {
        EnsureValidRequest();

        var result = await _service.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
