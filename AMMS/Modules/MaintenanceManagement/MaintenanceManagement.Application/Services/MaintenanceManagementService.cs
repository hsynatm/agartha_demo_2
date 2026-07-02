using AutoMapper;
using AMMS.Core.Exceptions;
using AMMS.Core.Localization;
using AMMS.Core.Services;
using AMMS.Shared.Models;
using MaintenanceManagement.Application.Dtos;
using MaintenanceManagement.Domain.Enums;
using MaintenanceManagement.Domain.Persistence;

namespace MaintenanceManagement.Application.Services;

public class MaintenanceManagementService : IMaintenanceManagementService
{
    private readonly IMaintenanceManagementUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public MaintenanceManagementService(IMaintenanceManagementUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<WorkOrderDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await EntityServiceHelpers.RequireAsync(
            _unitOfWork.WorkOrders.GetByIdWithDetailsAsync,
            id,
            LocalizationKeys.Modules.MaintenanceManagement.NotFound,
            LocalizationKeys.Modules.MaintenanceManagement.ErrorCodes.NotFound,
            cancellationToken);

        return _mapper.Map<WorkOrderDto>(entity);
    }

    public async Task<PagedResult<WorkOrderDto>> GetPagedAsync(
        PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        var paged = await _unitOfWork.WorkOrders.GetPagedAsync(request.Page, request.PageSize, cancellationToken);
        return PagedResult<WorkOrderDto>.WithMappedItems(paged, _mapper.Map<List<WorkOrderDto>>(paged.Items));
    }

    public async Task<WorkOrderDto> CreateAsync(WorkOrderDto request, CancellationToken cancellationToken = default)
    {
        ValidatePlannedStartDate(request.PlannedStartDate);

        var entity = _mapper.Map<Domain.Entities.WorkOrder>(request);

        await _unitOfWork.WorkOrders.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<WorkOrderDto>(entity);
    }

    public async Task<WorkOrderDto> UpdateAsync(
        Guid id,
        WorkOrderDto request,
        CancellationToken cancellationToken = default)
    {
        var entity = await EntityServiceHelpers.RequireAsync(
            _unitOfWork.WorkOrders.GetByIdAsync,
            id,
            LocalizationKeys.Modules.MaintenanceManagement.NotFound,
            LocalizationKeys.Modules.MaintenanceManagement.ErrorCodes.NotFound,
            cancellationToken);

        if (entity.Status is WorkOrderStatus.Closed or WorkOrderStatus.Cancelled)
        {
            throw new AmmsException.Business(
                LocalizationKeys.Modules.MaintenanceManagement.UpdateFinal,
                errorCode: LocalizationKeys.Modules.MaintenanceManagement.ErrorCodes.UpdateFinal,
                details: new { EntityId = id });
        }

        ValidatePlannedStartDate(request.PlannedStartDate);

        _mapper.Map(request, entity);
        _unitOfWork.WorkOrders.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<WorkOrderDto>(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await EntityServiceHelpers.RequireAsync(
            _unitOfWork.WorkOrders.GetByIdAsync,
            id,
            LocalizationKeys.Modules.MaintenanceManagement.NotFound,
            LocalizationKeys.Modules.MaintenanceManagement.ErrorCodes.NotFound,
            cancellationToken);

        _unitOfWork.WorkOrders.Remove(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static void ValidatePlannedStartDate(DateTime plannedStartDate)
    {
        if (plannedStartDate.Date < DateTime.UtcNow.Date)
        {
            throw new AmmsException.Business(
                LocalizationKeys.Modules.MaintenanceManagement.PastDate,
                errorCode: LocalizationKeys.Modules.MaintenanceManagement.ErrorCodes.PastDate);
        }
    }
}
