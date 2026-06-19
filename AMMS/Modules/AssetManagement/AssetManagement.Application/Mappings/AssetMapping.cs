using AutoMapper;
using AssetManagement.Application.Dtos;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;

namespace AssetManagement.Application.Mappings;

public class AssetMapping : Profile
{
    public AssetMapping()
    {
        CreateMap<AssetLocation, AssetLocationDto>();
        CreateMap<AssetDocument, AssetDocumentDto>();
        CreateMap<AssetLifeLimit, AssetLifeLimitDto>();
        CreateMap<AssetPart, AssetPartDto>();
        CreateMap<AssetStatusHistory, AssetStatusHistoryDto>();
        CreateMap<AssetLocationHistory, AssetLocationHistoryDto>();
        CreateMap<Asset, AssetDto>();

        CreateMap<AssetDto, Asset>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.CurrentLocation, opt => opt.Ignore())
            .ForMember(dest => dest.Documents, opt => opt.Ignore())
            .ForMember(dest => dest.LocationHistories, opt => opt.Ignore())
            .ForMember(dest => dest.StatusHistories, opt => opt.Ignore())
            .ForMember(dest => dest.Parts, opt => opt.Ignore())
            .ForMember(dest => dest.LifeLimits, opt => opt.Ignore())
            .ForMember(dest => dest.AssetCode, opt => opt.MapFrom(src => src.AssetCode.Trim()))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.Trim()))
            .ForMember(
                dest => dest.Status,
                opt => opt.MapFrom(src => src.Status == 0 ? AssetStatus.InService : src.Status));
    }
}
