using AMMS.Shared.Dtos;
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
    public async Task<ActionResult<PagedResult<FaultReportDto>>> GetPaged([FromQuery] PagedRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.GetPagedAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FaultReportDto>> GetById(Guid id, CancellationToken cancellationToken)
    {

        Convert.ToInt32("ffff");

        var result = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<FaultReportDto>> Create([FromBody] FaultReportDto request,CancellationToken cancellationToken)
    {
        EnsureValidRequest(request);
        var result = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<FaultReportDto>> Update(Guid id,[FromBody] FaultReportDto request,CancellationToken cancellationToken)
    {
        EnsureValidRequest(request);

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
