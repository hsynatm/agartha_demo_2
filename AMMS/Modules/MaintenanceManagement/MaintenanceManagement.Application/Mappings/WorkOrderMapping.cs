using AutoMapper;
using MaintenanceManagement.Application.Dtos;
using MaintenanceManagement.Domain.Entities;
using MaintenanceManagement.Domain.Enums;

namespace MaintenanceManagement.Application.Mappings;

public class WorkOrderMapping : Profile
{
    public WorkOrderMapping()
    {
        CreateMap<WorkOrderTask, WorkOrderTaskDto>();
        CreateMap<WorkOrderAssignment, WorkOrderAssignmentDto>();
        CreateMap<WorkOrderMaterial, WorkOrderMaterialDto>();
        CreateMap<WorkOrderTool, WorkOrderToolDto>();
        CreateMap<WorkOrderApproval, WorkOrderApprovalDto>();
        CreateMap<WorkOrderAttachment, WorkOrderAttachmentDto>();
        CreateMap<WorkOrderStatusHistory, WorkOrderStatusHistoryDto>();
        CreateMap<WorkOrder, WorkOrderDto>();

        CreateMap<WorkOrderDto, WorkOrder>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Tasks, opt => opt.Ignore())
            .ForMember(dest => dest.Assignments, opt => opt.Ignore())
            .ForMember(dest => dest.Materials, opt => opt.Ignore())
            .ForMember(dest => dest.Tools, opt => opt.Ignore())
            .ForMember(dest => dest.Approvals, opt => opt.Ignore())
            .ForMember(dest => dest.Attachments, opt => opt.Ignore())
            .ForMember(dest => dest.StatusHistories, opt => opt.Ignore())
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title.Trim()))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description.Trim()))
            .ForMember(
                dest => dest.Status,
                opt => opt.MapFrom(src => src.Status == 0 ? WorkOrderStatus.Open : src.Status));
    }
}
