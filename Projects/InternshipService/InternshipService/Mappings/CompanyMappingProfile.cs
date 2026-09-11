using AutoMapper;
using InternshipService.Models.DTO;
using InternshipService.Models.Entities;

namespace InternshipService.Mappings;

public class CompanyMappingProfile : Profile
{
    public CompanyMappingProfile()
    {
        CreateMap<CompanyRequestModel, Company>()
            .ForMember(destination => destination.Id, options => options.Ignore())
            .ForMember(destination => destination.CreatedDate, options => options.Ignore())
            .ForMember(destination => destination.Vacancies, options => options.Ignore());
        
        CreateMap<Company, CompanyResponseModel>()
            .ForMember(
                destination => destination.CreatedDate,
                options => options.MapFrom(source => source.CreatedDate.ToString("yyyy-MM-ddTHH:mm:ssZ"))
            );
    }
}

