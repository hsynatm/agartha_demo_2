using AutoMapper;
using AMMS.Core.Exceptions;
using AMMS.Core.Localization;
using AMMS.Shared.Models;
using UserManagement.Application.Dtos;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Persistence;

namespace UserManagement.Application.Services;

public sealed class RoleGroupService : IRoleGroupService
{
    private readonly IUserManagementUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RoleGroupService(IUserManagementUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<RoleGroupDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.RoleGroups.GetByIdWithRolesAsync(id, cancellationToken);
        if (entity is null)
        {
            throw AmmsException.NotFound.ForEntity(
                LocalizationKeys.Modules.UserManagement.RoleGroupNotFound,
                LocalizationKeys.Modules.UserManagement.ErrorCodes.RoleGroupNotFound,
                id);
        }

        return MapRoleGroup(entity);
    }

    public async Task<PagedResult<RoleGroupDto>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        var paged = await _unitOfWork.RoleGroups.GetPagedAsync(request.Page, request.PageSize, cancellationToken);
        var items = new List<RoleGroupDto>(paged.Items.Count);

        foreach (var roleGroup in paged.Items)
        {
            var detailed = await _unitOfWork.RoleGroups.GetByIdWithRolesAsync(roleGroup.Id, cancellationToken);
            items.Add(MapRoleGroup(detailed ?? roleGroup));
        }

        return new PagedResult<RoleGroupDto>
        {
            Items = items,
            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalCount = paged.TotalCount
        };
    }

    public async Task<RoleGroupDto> CreateAsync(CreateRoleGroupDto request, CancellationToken cancellationToken = default)
    {
        var code = request.Code.Trim();
        if (await _unitOfWork.RoleGroups.CodeExistsAsync(code, cancellationToken: cancellationToken))
        {
            throw new AmmsException.Business(
                LocalizationKeys.Modules.UserManagement.DuplicateRoleGroupCode,
                messageArgs: new { Code = code },
                errorCode: LocalizationKeys.Modules.UserManagement.ErrorCodes.DuplicateRoleGroupCode);
        }

        var roleIds = await ResolveRoleIdsAsync(request.Roles, cancellationToken);
        var entity = _mapper.Map<RoleGroup>(request);
        entity.Code = code;

        await _unitOfWork.RoleGroups.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _unitOfWork.RoleGroups.ReplaceRoleAssignmentsAsync(entity.Id, roleIds, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    public async Task<RoleGroupDto> UpdateAsync(Guid id, UpdateRoleGroupDto request, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.RoleGroups.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            throw AmmsException.NotFound.ForEntity(
                LocalizationKeys.Modules.UserManagement.RoleGroupNotFound,
                LocalizationKeys.Modules.UserManagement.ErrorCodes.RoleGroupNotFound,
                id);
        }

        var roleIds = await ResolveRoleIdsAsync(request.Roles, cancellationToken);
        _mapper.Map(request, entity);
        await _unitOfWork.RoleGroups.ReplaceRoleAssignmentsAsync(entity.Id, roleIds, cancellationToken);
        _unitOfWork.RoleGroups.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.RoleGroups.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            throw AmmsException.NotFound.ForEntity(
                LocalizationKeys.Modules.UserManagement.RoleGroupNotFound,
                LocalizationKeys.Modules.UserManagement.ErrorCodes.RoleGroupNotFound,
                id);
        }

        _unitOfWork.RoleGroups.Remove(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<IReadOnlyCollection<Guid>> ResolveRoleIdsAsync(
        IReadOnlyCollection<string> roleCodes,
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

        return roleIds;
    }

    private RoleGroupDto MapRoleGroup(RoleGroup entity)
    {
        var dto = _mapper.Map<RoleGroupDto>(entity);
        dto.Roles = entity.RoleGroupRoles
            .Select(roleGroupRole => roleGroupRole.Role.Code)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(code => code, StringComparer.OrdinalIgnoreCase)
            .ToList();
        return dto;
    }
}
