using AutoMapper;
using AMMS.Core.Exceptions;
using AMMS.Core.Localization;
using AMMS.Shared.Models;
using UserManagement.Application.Dtos;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Persistence;

namespace UserManagement.Application.Services;

public sealed class RoleService : IRoleService
{
    private readonly IUserManagementUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RoleService(IUserManagementUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<RoleDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Roles.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            throw AmmsException.NotFound.ForEntity(
                LocalizationKeys.Modules.UserManagement.RoleNotFound,
                LocalizationKeys.Modules.UserManagement.ErrorCodes.RoleNotFound,
                id);
        }

        return _mapper.Map<RoleDto>(entity);
    }

    public async Task<PagedResult<RoleDto>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        var paged = await _unitOfWork.Roles.GetPagedAsync(request.Page, request.PageSize, cancellationToken);
        return new PagedResult<RoleDto>
        {
            Items = _mapper.Map<List<RoleDto>>(paged.Items),
            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalCount = paged.TotalCount
        };
    }

    public async Task<RoleDto> CreateAsync(CreateRoleDto request, CancellationToken cancellationToken = default)
    {
        var code = request.Code.Trim();
        if (await _unitOfWork.Roles.CodeExistsAsync(code, cancellationToken: cancellationToken))
        {
            throw new AmmsException.Business(
                LocalizationKeys.Modules.UserManagement.DuplicateRoleCode,
                messageArgs: new { Code = code },
                errorCode: LocalizationKeys.Modules.UserManagement.ErrorCodes.DuplicateRoleCode);
        }

        var entity = _mapper.Map<Role>(request);
        entity.Code = code;

        await _unitOfWork.Roles.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<RoleDto>(entity);
    }

    public async Task<RoleDto> UpdateAsync(Guid id, UpdateRoleDto request, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Roles.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            throw AmmsException.NotFound.ForEntity(
                LocalizationKeys.Modules.UserManagement.RoleNotFound,
                LocalizationKeys.Modules.UserManagement.ErrorCodes.RoleNotFound,
                id);
        }

        _mapper.Map(request, entity);
        _unitOfWork.Roles.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<RoleDto>(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Roles.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            throw AmmsException.NotFound.ForEntity(
                LocalizationKeys.Modules.UserManagement.RoleNotFound,
                LocalizationKeys.Modules.UserManagement.ErrorCodes.RoleNotFound,
                id);
        }

        _unitOfWork.Roles.Remove(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
