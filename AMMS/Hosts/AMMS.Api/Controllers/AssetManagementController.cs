using AMMS.Shared.Models;
using AssetManagement.Application.Dtos;
using AssetManagement.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AMMS.Api.Controllers;

[Route("api/v1/asset-management/[action]")]
public class AssetManagementController : ApiBaseController
{
    private readonly IAssetManagementService _service;

    public AssetManagementController(IAssetManagementService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<AssetDto>>> GetPaged([FromQuery] PagedRequest request,CancellationToken cancellationToken)
    {
        var result = await _service.GetPagedAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AssetDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<AssetDto>> Create([FromBody] AssetDto request, CancellationToken cancellationToken)
    {
        EnsureValidRequest(request);
        var result = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AssetDto>> Update(Guid id,[FromBody] AssetDto request, CancellationToken cancellationToken)
    {
        var resultMevcut = await _service.GetByIdAsync(id, cancellationToken);
        resultMevcut.Name= request.Name;    
        resultMevcut.AssetCode= request.AssetCode;
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
