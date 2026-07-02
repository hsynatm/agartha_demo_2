using AutoMapper;
using AMMS.Core.Exceptions;
using AMMS.Core.Localization;
using AMMS.Core.Services;
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
        var entity = await EntityServiceHelpers.RequireAsync(
            _unitOfWork.Users.GetByIdWithAssignmentsAsync,
            id,
            LocalizationKeys.Modules.UserManagement.NotFound,
            LocalizationKeys.Modules.UserManagement.ErrorCodes.NotFound,
            cancellationToken);

        return MapUser(entity);
    }

    public async Task<PagedResult<UserDto>> GetPagedAsync(
        PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        var paged = await _unitOfWork.Users.GetPagedWithAssignmentsAsync(
            request.Page,
            request.PageSize,
            cancellationToken);

        var items = paged.Items.Select(MapUser).ToList();
        return PagedResult<UserDto>.WithMappedItems(paged, items);
    }

    public async Task<UserDto> CreateAsync(CreateUserDto request, CancellationToken cancellationToken = default)
    {
        var normalizedUsername = request.Username.Trim();
        var password = request.Password.Trim();
        EnsureValidUserInput(normalizedUsername, request.Email.Trim(), password);

        await RemoveDeletedUsernameConflictAsync(normalizedUsername, cancellationToken);

        var (roleIds, roleGroupIds) = await ResolveAssignmentsAsync(request.Roles, request.RoleGroups, cancellationToken);
        EnsureHasAuthorization(roleIds, roleGroupIds);

        var entity = await PersistNewUserAsync(request, normalizedUsername, roleIds, roleGroupIds, cancellationToken);
        await SyncKeycloakOnCreateAsync(entity, password, cancellationToken);

        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    public async Task<UserDto> UpdateAsync(
        Guid id,
        UpdateUserDto request,
        CancellationToken cancellationToken = default)
    {
        var entity = await EntityServiceHelpers.RequireAsync(
            _unitOfWork.Users.GetByIdAsync,
            id,
            LocalizationKeys.Modules.UserManagement.NotFound,
            LocalizationKeys.Modules.UserManagement.ErrorCodes.NotFound,
            cancellationToken);

        var (roleIds, roleGroupIds) = await ResolveAssignmentsAsync(request.Roles, request.RoleGroups, cancellationToken);
        EnsureHasAuthorization(roleIds, roleGroupIds);

        _mapper.Map(request, entity);
        EnsureValidUserInput(entity.Username, request.Email.Trim());
        await _unitOfWork.Users.ReplaceRoleAssignmentsAsync(entity.Id, roleIds, roleGroupIds, cancellationToken);

        if (!string.IsNullOrWhiteSpace(entity.KeycloakUserId))
        {
            await SyncKeycloakOnUpdateAsync(entity, request.Password, cancellationToken);
        }

        _unitOfWork.Users.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await EntityServiceHelpers.RequireAsync(
            _unitOfWork.Users.GetByIdAsync,
            id,
            LocalizationKeys.Modules.UserManagement.NotFound,
            LocalizationKeys.Modules.UserManagement.ErrorCodes.NotFound,
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(entity.KeycloakUserId))
        {
            await _keycloakUserSyncService.DeleteUserAsync(entity.KeycloakUserId, cancellationToken);
        }

        _unitOfWork.Users.Remove(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task RemoveDeletedUsernameConflictAsync(string normalizedUsername, CancellationToken cancellationToken)
    {
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
    }

    private async Task<AppUser> PersistNewUserAsync(
        CreateUserDto request,
        string normalizedUsername,
        IReadOnlyCollection<Guid> roleIds,
        IReadOnlyCollection<Guid> roleGroupIds,
        CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<AppUser>(request);
        entity.Username = normalizedUsername;

        await _unitOfWork.Users.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _unitOfWork.Users.ReplaceRoleAssignmentsAsync(entity.Id, roleIds, roleGroupIds, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity;
    }

    private async Task SyncKeycloakOnCreateAsync(AppUser entity, string password, CancellationToken cancellationToken)
    {
        try
        {
            entity.KeycloakUserId = await _keycloakUserSyncService.CreateUserAsync(
                entity,
                password,
                cancellationToken);
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
    }

    private async Task SyncKeycloakOnUpdateAsync(AppUser entity, string? password, CancellationToken cancellationToken)
    {
        try
        {
            var newPassword = string.IsNullOrWhiteSpace(password) ? null : password.Trim();
            await _keycloakUserSyncService.UpdateUserAsync(entity, newPassword, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            throw new AmmsException.Business(
                LocalizationKeys.Modules.UserManagement.KeycloakSyncFailed,
                messageArgs: new { Detail = ex.Message },
                errorCode: LocalizationKeys.Modules.UserManagement.ErrorCodes.KeycloakSyncFailed);
        }
    }

    private async Task<(IReadOnlyCollection<Guid> RoleIds, IReadOnlyCollection<Guid> RoleGroupIds)> ResolveAssignmentsAsync(
        IReadOnlyCollection<string> roleCodes,
        IReadOnlyCollection<string> roleGroupCodes,
        CancellationToken cancellationToken)
    {
        var roleIds = await RoleCodeResolver.ResolveRoleIdsAsync(_unitOfWork.Roles, roleCodes, cancellationToken);
        var roleGroupIds = await RoleCodeResolver.ResolveRoleGroupIdsAsync(_unitOfWork.RoleGroups, roleGroupCodes, cancellationToken);
        return (roleIds, roleGroupIds);
    }

    private static void EnsureValidUserInput(string username, string email, string? password = null)
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

        if (password is not null
            && !UserInputValidator.TryValidatePassword(password, out var passwordError))
        {
            throw new AmmsException.Business(
                LocalizationKeys.Modules.UserManagement.InvalidPassword,
                messageArgs: new { Detail = passwordError },
                errorCode: LocalizationKeys.Modules.UserManagement.ErrorCodes.InvalidPassword);
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
