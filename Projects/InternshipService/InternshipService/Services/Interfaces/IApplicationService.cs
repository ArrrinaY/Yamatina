using InternshipService.Models.DTO;

namespace InternshipService.Services;

public interface IApplicationService
{
    Task<List<ApplicationResponseModel>> GetAsync();
    Task<ApplicationResponseModel?> GetByIdAsync(int id);
    Task<List<ApplicationResponseModel>> GetByVacancyIdAsync(int vacancyId);
    Task<List<ApplicationResponseModel>> GetByCandidateIdAsync(int candidateId);
    Task<ApplicationResponseModel> CreateAsync(ApplicationRequestModel applicationRequestModel);
    Task UpdateStatusAsync(int id, int status);
    Task DeleteAsync(int id);
}

