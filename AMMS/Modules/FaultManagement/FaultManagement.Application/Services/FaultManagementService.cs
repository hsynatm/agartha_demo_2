using AutoMapper;
using AMMS.Core.Exceptions;
using AMMS.Core.Localization;
using AMMS.Shared.Models;
using FaultManagement.Application.Dtos;
using FaultManagement.Application.Services;
using FaultManagement.Domain.Entities;
using FaultManagement.Domain.Enums;
using FaultManagement.Domain.Persistence;

namespace FaultManagement.Application.Services;

public class FaultManagementService : IFaultManagementService
{
    private readonly IFaultManagementUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public FaultManagementService(IFaultManagementUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<FaultReportDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.FaultReports.GetByIdWithDetailsAsync(id, cancellationToken);
        if (entity is null)
        {
            throw AmmsException.NotFound.ForEntity(
                LocalizationKeys.Modules.FaultManagement.NotFound,
                LocalizationKeys.Modules.FaultManagement.ErrorCodes.NotFound,
                id);
        }

        return _mapper.Map<FaultReportDto>(entity);
    }

    public async Task<PagedResult<FaultReportDto>> GetPagedAsync(
        PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        var paged = await _unitOfWork.FaultReports.GetPagedAsync(request.Page, request.PageSize, cancellationToken);

        return new PagedResult<FaultReportDto>
        {
            Items = _mapper.Map<List<FaultReportDto>>(paged.Items),
            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalCount = paged.TotalCount
        };
    }

    public async Task<FaultReportDto> CreateAsync(
        FaultReportDto request,
        CancellationToken cancellationToken = default)
    {
        if (request.Status is FaultStatus.Closed)
        {
            throw new AmmsException.Business(
                LocalizationKeys.Modules.FaultManagement.CreateClosed,
                errorCode: LocalizationKeys.Modules.FaultManagement.ErrorCodes.CreateClosed);
        }

        var entity = _mapper.Map<FaultReport>(request);

        await _unitOfWork.FaultReports.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<FaultReportDto>(entity);
    }

    public async Task<FaultReportDto> UpdateAsync(
        Guid id,
        FaultReportDto request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.FaultReports.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            throw AmmsException.NotFound.ForEntity(
                LocalizationKeys.Modules.FaultManagement.NotFound,
                LocalizationKeys.Modules.FaultManagement.ErrorCodes.NotFound,
                id);
        }

        if (entity.Status == FaultStatus.Closed)
        {
            throw new AmmsException.Business(
                LocalizationKeys.Modules.FaultManagement.UpdateClosed,
                errorCode: LocalizationKeys.Modules.FaultManagement.ErrorCodes.UpdateClosed,
                details: new { EntityId = id });
        }

        _mapper.Map(request, entity);
        _unitOfWork.FaultReports.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<FaultReportDto>(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.FaultReports.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            throw AmmsException.NotFound.ForEntity(
                LocalizationKeys.Modules.FaultManagement.NotFound,
                LocalizationKeys.Modules.FaultManagement.ErrorCodes.NotFound,
                id);
        }

        _unitOfWork.FaultReports.Remove(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
