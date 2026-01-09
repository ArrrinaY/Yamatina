using InternshipService.Models.DTO;

namespace InternshipService.Services;

public interface ICandidateService
{
    Task<List<CandidateResponseModel>> GetAsync();
    Task<CandidateResponseModel?> GetByIdAsync(int id);
    Task<CandidateResponseModel> CreateAsync(CandidateRequestModel candidateRequestModel);
    Task UpdateAsync(int id, CandidateRequestModel candidateRequestModel);
    Task DeleteAsync(int id);
}

