using AMMS.Core.Authorization;
using AMMS.Infrastructure.Authentication;
using AMMS.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.Dtos;
using UserManagement.Application.Services;

namespace AMMS.Api.Controllers;

[Route("api/v1/role-groups/[action]")]
[AmmsAuthorize(RoleGroups = AmmsRoleGroupNames.UserManagement)]
public class RoleGroupManagementController : ApiBaseController
{
    private readonly IRoleGroupService _service;

    public RoleGroupManagementController(IRoleGroupService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<RoleGroupDto>>> GetPaged([FromQuery] PagedRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.GetPagedAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RoleGroupDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<RoleGroupDto>> Create([FromBody] CreateRoleGroupDto request, CancellationToken cancellationToken)
    {
        EnsureValidRequest(request);
        var result = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<RoleGroupDto>> Update(Guid id, [FromBody] UpdateRoleGroupDto request, CancellationToken cancellationToken)
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
