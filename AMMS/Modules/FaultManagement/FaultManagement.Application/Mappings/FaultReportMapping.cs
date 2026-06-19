using AutoMapper;
using FaultManagement.Application.Dtos;
using FaultManagement.Domain.Entities;
using FaultManagement.Domain.Enums;

namespace FaultManagement.Application.Mappings;

public class FaultReportMapping : Profile
{
    public FaultReportMapping()
    {
        CreateMap<FaultActivity, FaultActivityDto>();
        CreateMap<FaultAttachment, FaultAttachmentDto>();
        CreateMap<FaultRepairAction, FaultRepairActionDto>();
        CreateMap<FaultReport, FaultReportDto>();

        CreateMap<FaultReportDto, FaultReport>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Attachments, opt => opt.Ignore())
            .ForMember(dest => dest.Activities, opt => opt.Ignore())
            .ForMember(dest => dest.RepairActions, opt => opt.Ignore())
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title.Trim()))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description.Trim()))
            .ForMember(
                dest => dest.Status,
                opt => opt.MapFrom(src => src.Status == 0 ? FaultStatus.Reported : src.Status));
    }
}
