using AMMS.Core.Authorization;
using AMMS.Infrastructure.Authentication;
using AMMS.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.Dtos;
using UserManagement.Application.Services;

namespace AMMS.Api.Controllers;

[Route("api/v1/user-management/[action]")]
[AmmsAuthorize(RoleGroups = AmmsRoleGroupNames.UserManagement)]
public class UserManagementController : ApiBaseController
{
    private readonly IUserManagementService _service;

    public UserManagementController(IUserManagementService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<UserDto>>> GetPaged([FromQuery] PagedRequest request,CancellationToken cancellationToken)
    {
        var result = await _service.GetPagedAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserDto request,CancellationToken cancellationToken)
    {
        EnsureValidRequest(request);
        var result = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserDto>> Update(Guid id,[FromBody] UpdateUserDto request,CancellationToken cancellationToken)
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
