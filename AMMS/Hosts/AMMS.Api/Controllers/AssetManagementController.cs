using AMMS.Shared.Models;
using AssetManagement.Application.Dtos;
using AssetManagement.Application.Services;
using AssetManagement.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace AMMS.Api.Controllers;

[Route("api/v1/asset-management")]
public class AssetManagementController : ApiBaseController
{
    private readonly IAssetManagementService _service;

    public AssetManagementController(IAssetManagementService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<AssetDto>>> GetPaged(
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken)
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
    public async Task<ActionResult<AssetDto>> Create( [FromBody] AssetDto? request,   CancellationToken cancellationToken)
    {
        request ??= new AssetDto();
        ApplyCreateDemoDefaults(request);
        ClearCreateModelStateAfterDemoDefaults();

        EnsureValidRequest();

        var result = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    private static void ApplyCreateDemoDefaults(AssetDto request)
    {
        if (string.IsNullOrWhiteSpace(request.AssetCode))
        {
            request.AssetCode = "DEMO-AST-001";
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            request.Name = "Demo Asset";
        }

        if (request.Category == 0)
        {
            request.Category = AssetCategory.Aircraft;
        }

        if (request.Status == 0)
        {
            request.Status = AssetStatus.InService;
        }

        var demoAssetId = request.Id != Guid.Empty ? request.Id : Guid.NewGuid();

        var documentIndex = 0;
        foreach (var document in request.Documents)
        {
            ApplyDocumentDemoDefaults(document, demoAssetId, documentIndex);
            documentIndex++;
        }
    }

    private static void ApplyDocumentDemoDefaults(AssetDocumentDto document, Guid assetId, int index)
    {
        if (document.AssetId == Guid.Empty)
        {
            document.AssetId = assetId;
        }

        if (document.DocumentType == 0)
        {
            document.DocumentType = AssetDocumentType.Other;
        }

        if (string.IsNullOrWhiteSpace(document.DocumentNumber))
        {
            document.DocumentNumber = $"DEMO-DOC-{index + 1:000}";
        }

        if (string.IsNullOrWhiteSpace(document.FileName))
        {
            document.FileName = $"demo-document-{index + 1}.pdf";
        }

        if (string.IsNullOrWhiteSpace(document.FilePath))
        {
            document.FilePath = $"/demo/documents/demo-document-{index + 1}.pdf";
        }

        if (document.Version <= 0)
        {
            document.Version = 1;
        }
    }

    private void ClearCreateModelStateAfterDemoDefaults()
    {
        ClearModelStateError("request");
        ClearModelStateErrorsForPrefix("request");
        ClearModelStateErrorsForPrefix("AssetCode");
        ClearModelStateErrorsForPrefix("Name");
        ClearModelStateErrorsForPrefix("Category");
        ClearModelStateErrorsForPrefix("Status");
        ClearModelStateErrorsForPrefix("Documents");
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AssetDto>> Update(Guid id,[FromBody] AssetDto request, CancellationToken cancellationToken)
    {
        var resultMevcut = await _service.GetByIdAsync(id, cancellationToken);
        resultMevcut.Name= request.Name;    
        resultMevcut.AssetCode= request.AssetCode;
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
