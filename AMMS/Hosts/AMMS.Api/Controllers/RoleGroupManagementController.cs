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
    public Task<ActionResult<PagedResult<RoleGroupDto>>> GetPaged(
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken) =>
        OkPagedAsync(ct => _service.GetPagedAsync(request, ct), cancellationToken);

    [HttpGet("{id:guid}")]
    public Task<ActionResult<RoleGroupDto>> GetById(Guid id, CancellationToken cancellationToken) =>
        OkByIdAsync(_service.GetByIdAsync, id, cancellationToken);

    [HttpPost]
    public Task<ActionResult<RoleGroupDto>> Create(
        [FromBody] CreateRoleGroupDto request,
        CancellationToken cancellationToken) =>
        CreatedAtGetByIdAsync(request, _service.CreateAsync, result => result.Id, nameof(GetById), cancellationToken);

    [HttpPut("{id:guid}")]
    public Task<ActionResult<RoleGroupDto>> Update(
        Guid id,
        [FromBody] UpdateRoleGroupDto request,
        CancellationToken cancellationToken) =>
        OkValidatedAsync(request, _service.UpdateAsync, id, cancellationToken);

    [HttpDelete("{id:guid}")]
    public Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        NoContentDeleteAsync(_service.DeleteAsync, id, cancellationToken);
}
