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
    public Task<ActionResult<PagedResult<AssetDto>>> GetPaged(
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken) =>
        OkPagedAsync(ct => _service.GetPagedAsync(request, ct), cancellationToken);

    [HttpGet("{id:guid}")]
    public Task<ActionResult<AssetDto>> GetById(Guid id, CancellationToken cancellationToken) =>
        OkByIdAsync(_service.GetByIdAsync, id, cancellationToken);

    [HttpPost]
    public Task<ActionResult<AssetDto>> Create([FromBody] AssetDto request, CancellationToken cancellationToken) =>
        CreatedAtGetByIdAsync(request, _service.CreateAsync, result => result.Id, nameof(GetById), cancellationToken);

    [HttpPut("{id:guid}")]
    public Task<ActionResult<AssetDto>> Update(
        Guid id,
        [FromBody] AssetDto request,
        CancellationToken cancellationToken) =>
        OkValidatedAsync(request, _service.UpdateAsync, id, cancellationToken);

    [HttpDelete("{id:guid}")]
    public Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        NoContentDeleteAsync(_service.DeleteAsync, id, cancellationToken);
}
