using AutoMapper;
using InternshipService.Models.DTO;
using InternshipService.Models.Entities;

namespace InternshipService.Mappings;

public class TagMappingProfile : Profile
{
    public TagMappingProfile()
    {
        CreateMap<TagRequestModel, Tag>()
            .ForMember(destination => destination.Id, options => options.Ignore())
            .ForMember(destination => destination.VacancyTags, options => options.Ignore())
            .ForMember(destination => destination.Category, options => options.MapFrom(source => (TagCategory)source.Category));
        
        CreateMap<Tag, TagResponseModel>()
            .ForMember(destination => destination.Category, options => options.MapFrom(source => (int)source.Category));
    }
}

