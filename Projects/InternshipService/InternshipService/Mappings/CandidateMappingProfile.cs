using AutoMapper;
using InternshipService.Models.DTO;
using InternshipService.Models.Entities;

namespace InternshipService.Mappings;

public class CandidateMappingProfile : Profile
{
    public CandidateMappingProfile()
    {
        CreateMap<CandidateRequestModel, Candidate>()
            .ForMember(destination => destination.Id, options => options.Ignore())
            .ForMember(destination => destination.Applications, options => options.Ignore())
            .ForMember(
                destination => destination.ExperienceLevel,
                options => options.MapFrom(source => source.ExperienceLevel.HasValue 
                    ? (ExperienceLevel?)source.ExperienceLevel.Value 
                    : null)
            );
        
        CreateMap<Candidate, CandidateResponseModel>()
            .ForMember(
                destination => destination.ExperienceLevel,
                options => options.MapFrom(source => source.ExperienceLevel.HasValue 
                    ? (int?)source.ExperienceLevel.Value 
                    : null)
            );
    }
}

