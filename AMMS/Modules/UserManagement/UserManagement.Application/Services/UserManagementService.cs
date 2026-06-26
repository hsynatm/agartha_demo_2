using AutoMapper;
using AMMS.Core.Exceptions;
using AMMS.Core.Localization;
using AMMS.Shared.Models;
using UserManagement.Application.Dtos;
using UserManagement.Application.Validation;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Persistence;

namespace UserManagement.Application.Services;

public class UserManagementService : IUserManagementService
{
    private readonly IUserManagementUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IKeycloakUserSyncService _keycloakUserSyncService;

    public UserManagementService(
        IUserManagementUnitOfWork unitOfWork,
        IMapper mapper,
        IKeycloakUserSyncService keycloakUserSyncService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _keycloakUserSyncService = keycloakUserSyncService;
    }

    public async Task<UserDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Users.GetByIdWithAssignmentsAsync(id, cancellationToken);
        if (entity is null)
        {
            throw AmmsException.NotFound.ForEntity(
                LocalizationKeys.Modules.UserManagement.NotFound,
                LocalizationKeys.Modules.UserManagement.ErrorCodes.NotFound,
                id);
        }

        return MapUser(entity);
    }

    public async Task<PagedResult<UserDto>> GetPagedAsync(
        PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        var paged = await _unitOfWork.Users.GetPagedAsync(request.Page, request.PageSize, cancellationToken);
        var items = new List<UserDto>(paged.Items.Count);

        foreach (var user in paged.Items)
        {
            var detailed = await _unitOfWork.Users.GetByIdWithAssignmentsAsync(user.Id, cancellationToken);
            items.Add(MapUser(detailed ?? user));
        }

        return new PagedResult<UserDto>
        {
            Items = items,
            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalCount = paged.TotalCount
        };
    }

    public async Task<UserDto> CreateAsync(CreateUserDto request, CancellationToken cancellationToken = default)
    {
        var normalizedUsername = request.Username.Trim();
        EnsureValidUserInput(normalizedUsername, request.Email.Trim());

        var existingUser = await _unitOfWork.Users.GetByUsernameIncludingDeletedAsync(normalizedUsername, cancellationToken);
        if (existingUser is { IsDeleted: false })
        {
            throw new AmmsException.Business(
                LocalizationKeys.Modules.UserManagement.DuplicateUsername,
                messageArgs: new { Username = normalizedUsername },
                errorCode: LocalizationKeys.Modules.UserManagement.ErrorCodes.DuplicateUsername);
        }

        if (existingUser is { IsDeleted: true })
        {
            await _unitOfWork.Users.HardDeleteAsync(existingUser.Id, cancellationToken);
        }

        var (roleIds, roleGroupIds) = await ResolveAssignmentsAsync(request.Roles, request.RoleGroups, cancellationToken);
        EnsureHasAuthorization(roleIds, roleGroupIds);

        var entity = _mapper.Map<AppUser>(request);
        entity.Username = normalizedUsername;

        await _unitOfWork.Users.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _unitOfWork.Users.ReplaceRoleAssignmentsAsync(entity.Id, roleIds, roleGroupIds, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            entity.KeycloakUserId = await _keycloakUserSyncService.CreateUserAsync(entity, request.Password, cancellationToken);
            _unitOfWork.Users.Update(entity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            await _unitOfWork.Users.HardDeleteAsync(entity.Id, cancellationToken);
            throw new AmmsException.Business(
                LocalizationKeys.Modules.UserManagement.KeycloakSyncFailed,
                messageArgs: new { Detail = ex.Message },
                errorCode: LocalizationKeys.Modules.UserManagement.ErrorCodes.KeycloakSyncFailed);
        }

        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    public async Task<UserDto> UpdateAsync(
        Guid id,
        UpdateUserDto request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            throw AmmsException.NotFound.ForEntity(
                LocalizationKeys.Modules.UserManagement.NotFound,
                LocalizationKeys.Modules.UserManagement.ErrorCodes.NotFound,
                id);
        }

        var (roleIds, roleGroupIds) = await ResolveAssignmentsAsync(request.Roles, request.RoleGroups, cancellationToken);
        EnsureHasAuthorization(roleIds, roleGroupIds);

        _mapper.Map(request, entity);
        EnsureValidUserInput(entity.Username, request.Email.Trim());
        await _unitOfWork.Users.ReplaceRoleAssignmentsAsync(entity.Id, roleIds, roleGroupIds, cancellationToken);

        if (!string.IsNullOrWhiteSpace(entity.KeycloakUserId))
        {
            try
            {
                await _keycloakUserSyncService.UpdateUserAsync(entity, request.Password, cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                throw new AmmsException.Business(
                    LocalizationKeys.Modules.UserManagement.KeycloakSyncFailed,
                    messageArgs: new { Detail = ex.Message },
                    errorCode: LocalizationKeys.Modules.UserManagement.ErrorCodes.KeycloakSyncFailed);
            }
        }

        _unitOfWork.Users.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            throw AmmsException.NotFound.ForEntity(
                LocalizationKeys.Modules.UserManagement.NotFound,
                LocalizationKeys.Modules.UserManagement.ErrorCodes.NotFound,
                id);
        }

        if (!string.IsNullOrWhiteSpace(entity.KeycloakUserId))
        {
            await _keycloakUserSyncService.DeleteUserAsync(entity.KeycloakUserId, cancellationToken);
        }

        _unitOfWork.Users.Remove(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<(IReadOnlyCollection<Guid> RoleIds, IReadOnlyCollection<Guid> RoleGroupIds)> ResolveAssignmentsAsync(
        IReadOnlyCollection<string> roleCodes,
        IReadOnlyCollection<string> roleGroupCodes,
        CancellationToken cancellationToken)
    {
        var roleIds = new List<Guid>();
        foreach (var code in roleCodes.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var role = await _unitOfWork.Roles.GetByCodeAsync(code, cancellationToken);
            if (role is null)
            {
                throw new AmmsException.Business(
                    LocalizationKeys.Modules.UserManagement.RoleNotFound,
                    messageArgs: new { Code = code },
                    errorCode: LocalizationKeys.Modules.UserManagement.ErrorCodes.RoleNotFound);
            }

            roleIds.Add(role.Id);
        }

        var roleGroupIds = new List<Guid>();
        foreach (var code in roleGroupCodes.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var roleGroup = await _unitOfWork.RoleGroups.GetByCodeAsync(code, cancellationToken);
            if (roleGroup is null)
            {
                throw new AmmsException.Business(
                    LocalizationKeys.Modules.UserManagement.RoleGroupNotFound,
                    messageArgs: new { Code = code },
                    errorCode: LocalizationKeys.Modules.UserManagement.ErrorCodes.RoleGroupNotFound);
            }

            roleGroupIds.Add(roleGroup.Id);
        }

        return (roleIds, roleGroupIds);
    }

    private static void EnsureValidUserInput(string username, string email)
    {
        if (!UserInputValidator.TryValidateUsername(username, out var usernameError))
        {
            throw new AmmsException.Business(
                LocalizationKeys.Modules.UserManagement.InvalidUsername,
                messageArgs: new { Detail = usernameError },
                errorCode: LocalizationKeys.Modules.UserManagement.ErrorCodes.InvalidUsername);
        }

        if (!UserInputValidator.TryValidateEmail(email, out var emailError))
        {
            throw new AmmsException.Business(
                LocalizationKeys.Modules.UserManagement.InvalidEmail,
                messageArgs: new { Detail = emailError },
                errorCode: LocalizationKeys.Modules.UserManagement.ErrorCodes.InvalidEmail);
        }
    }

    private static void EnsureHasAuthorization(
        IReadOnlyCollection<Guid> roleIds,
        IReadOnlyCollection<Guid> roleGroupIds)
    {
        if (roleIds.Count > 0 || roleGroupIds.Count > 0)
        {
            return;
        }

        throw new AmmsException.Business(
            LocalizationKeys.Modules.UserManagement.InvalidRoles,
            errorCode: LocalizationKeys.Modules.UserManagement.ErrorCodes.InvalidRoles);
    }

    private UserDto MapUser(AppUser entity)
    {
        var dto = _mapper.Map<UserDto>(entity);
        dto.Roles = entity.UserRoles
            .Select(userRole => userRole.Role.Code)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(code => code, StringComparer.OrdinalIgnoreCase)
            .ToList();
        dto.RoleGroups = entity.UserRoleGroups
            .Select(userRoleGroup => userRoleGroup.RoleGroup.Code)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(code => code, StringComparer.OrdinalIgnoreCase)
            .ToList();
        return dto;
    }
}
