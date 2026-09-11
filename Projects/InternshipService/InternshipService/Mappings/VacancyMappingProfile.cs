using AutoMapper;
using InternshipService.Models.DTO;
using InternshipService.Models.Entities;

namespace InternshipService.Mappings;

public class VacancyMappingProfile : Profile
{
    public VacancyMappingProfile()
    {
        CreateMap<VacancyRequestModel, Vacancy>()
            .ForMember(destination => destination.Id, options => options.Ignore())
            .ForMember(destination => destination.CreatedDate, options => options.Ignore())
            .ForMember(destination => destination.Company, options => options.Ignore())
            .ForMember(destination => destination.Applications, options => options.Ignore())
            .ForMember(destination => destination.VacancyTags, options => options.Ignore())
            .ForMember(destination => destination.Type, options => options.MapFrom(source => (VacancyType)source.Type));
        
        CreateMap<Vacancy, VacancyResponseModel>()
            .ForMember(
                destination => destination.CreatedDate,
                options => options.MapFrom(source => source.CreatedDate.ToString("yyyy-MM-ddTHH:mm:ssZ"))
            )
            .ForMember(destination => destination.Type, options => options.MapFrom(source => (int)source.Type))
            .ForMember(
                destination => destination.Tags,
                options => options.MapFrom(source => source.VacancyTags != null 
                    ? source.VacancyTags.Where(vt => vt.Tag != null).Select(vt => vt.Tag).ToList()
                    : new List<Tag>())
            );
    }
}

