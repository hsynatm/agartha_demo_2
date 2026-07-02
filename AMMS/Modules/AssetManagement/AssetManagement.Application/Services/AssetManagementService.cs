using AutoMapper;
using AMMS.Core.Exceptions;
using AMMS.Core.Localization;
using AMMS.Core.Services;
using AMMS.Shared.Models;
using AssetManagement.Application.Dtos;
using AssetManagement.Domain.Persistence;

namespace AssetManagement.Application.Services;

public class AssetManagementService : IAssetManagementService
{
    private readonly IAssetManagementUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AssetManagementService(IAssetManagementUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<AssetDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await EntityServiceHelpers.RequireAsync(
            _unitOfWork.Assets.GetByIdWithDetailsAsync,
            id,
            LocalizationKeys.Modules.AssetManagement.NotFound,
            LocalizationKeys.Modules.AssetManagement.ErrorCodes.NotFound,
            cancellationToken);

        return _mapper.Map<AssetDto>(entity);
    }

    public async Task<PagedResult<AssetDto>> GetPagedAsync(
        PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        var paged = await _unitOfWork.Assets.GetPagedAsync(request.Page, request.PageSize, cancellationToken);
        return PagedResult<AssetDto>.WithMappedItems(paged, _mapper.Map<List<AssetDto>>(paged.Items));
    }

    public async Task<AssetDto> CreateAsync(AssetDto request, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.Map<Domain.Entities.Asset>(request);

        await _unitOfWork.Assets.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<AssetDto>(entity);
    }

    public async Task<AssetDto> UpdateAsync(
        Guid id,
        AssetDto request,
        CancellationToken cancellationToken = default)
    {
        var entity = await EntityServiceHelpers.RequireAsync(
            _unitOfWork.Assets.GetByIdAsync,
            id,
            LocalizationKeys.Modules.AssetManagement.NotFound,
            LocalizationKeys.Modules.AssetManagement.ErrorCodes.NotFound,
            cancellationToken);

        _mapper.Map(request, entity);
        _unitOfWork.Assets.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<AssetDto>(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await EntityServiceHelpers.RequireAsync(
            _unitOfWork.Assets.GetByIdAsync,
            id,
            LocalizationKeys.Modules.AssetManagement.NotFound,
            LocalizationKeys.Modules.AssetManagement.ErrorCodes.NotFound,
            cancellationToken);

        _unitOfWork.Assets.Remove(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
