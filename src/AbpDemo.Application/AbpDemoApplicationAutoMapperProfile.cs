using System.Linq;
using AbpDemo.BasicData.WorkCenters.Aggregates;
using AbpDemo.BasicData.WorkCenters.Dtos;
using AbpDemo.BasicData.Workshops.Aggregates;
using AbpDemo.BasicData.Workshops.Dtos;
using AbpDemo.Domain.Shared.ValueObjects;
using AbpDemo.Engineering.Processes;
using AbpDemo.Engineering.Processes.Dtos;
using AbpDemo.Engineering.Products;
using AbpDemo.Engineering.Products.Dtos;
using AutoMapper;

namespace AbpDemo;

public class AbpDemoApplicationAutoMapperProfile : Profile
{
    public AbpDemoApplicationAutoMapperProfile()
    {
        CreateMap<Product, ProductDto>();
        CreateMap<ProductSpecification, ProductSpecificationDto>();
        CreateMap<BomItem, BomItemDto>();
        CreateMap<ProductVersion, ProductVersionDto>();

        // ProcessRoute 映射
        CreateMap<ProcessRoute, ProcessRouteDto>()
            .ForMember(dest => dest.Steps, opt => opt.MapFrom(src => src.Steps.OrderBy(s => s.Sequence).ToList()));

        // ProcessStep 映射
        CreateMap<ProcessStep, ProcessStepDto>()
            .ForMember(dest => dest.IsKeyProcess, opt => opt.MapFrom(src => src.IsCritical))
            .ForMember(dest => dest.Description, opt => opt.Ignore());

        // BasicData 映射
        CreateMap<Workshop, WorkshopDto>();
        CreateMap<WorkCenter, WorkCenterDto>();
    }
}