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
    public Task<ActionResult<PagedResult<UserDto>>> GetPaged(
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken) =>
        OkPagedAsync(ct => _service.GetPagedAsync(request, ct), cancellationToken);

    [HttpGet("{id:guid}")]
    public Task<ActionResult<UserDto>> GetById(Guid id, CancellationToken cancellationToken) =>
        OkByIdAsync(_service.GetByIdAsync, id, cancellationToken);

    [HttpPost]
    public Task<ActionResult<UserDto>> Create(
        [FromBody] CreateUserDto request,
        CancellationToken cancellationToken) =>
        CreatedAtGetByIdAsync(request, _service.CreateAsync, result => result.Id, nameof(GetById), cancellationToken);

    [HttpPut("{id:guid}")]
    public Task<ActionResult<UserDto>> Update(
        Guid id,
        [FromBody] UpdateUserDto request,
        CancellationToken cancellationToken) =>
        OkValidatedAsync(request, _service.UpdateAsync, id, cancellationToken);

    [HttpDelete("{id:guid}")]
    public Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        NoContentDeleteAsync(_service.DeleteAsync, id, cancellationToken);
}
