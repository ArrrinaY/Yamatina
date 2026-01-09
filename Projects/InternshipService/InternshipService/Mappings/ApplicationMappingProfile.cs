using AutoMapper;
using InternshipService.Models.DTO;
using InternshipService.Models.Entities;

namespace InternshipService.Mappings;

public class ApplicationMappingProfile : Profile
{
    public ApplicationMappingProfile()
    {
        CreateMap<ApplicationRequestModel, Application>()
            .ForMember(destination => destination.Id, options => options.Ignore())
            .ForMember(destination => destination.Status, options => options.Ignore())
            .ForMember(destination => destination.AppliedDate, options => options.Ignore())
            .ForMember(destination => destination.Candidate, options => options.Ignore())
            .ForMember(destination => destination.Vacancy, options => options.Ignore());
        
        CreateMap<Application, ApplicationResponseModel>()
            .ForMember(
                destination => destination.AppliedDate,
                options => options.MapFrom(source => source.AppliedDate.ToString("yyyy-MM-ddTHH:mm:ssZ"))
            )
            .ForMember(destination => destination.Status, options => options.MapFrom(source => (int)source.Status));
    }
}

