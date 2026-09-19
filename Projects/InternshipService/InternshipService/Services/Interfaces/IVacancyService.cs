using InternshipService.Models.DTO;

namespace InternshipService.Services;

public interface IVacancyService
{
    Task<PaginatedResponse<VacancyResponseModel>> GetFilteredAsync(VacancyFilterModel filter);
    Task<List<VacancyResponseModel>> GetByCompanyIdAsync(int companyId);
    Task<VacancyResponseModel?> GetByIdAsync(int id);
    Task<VacancyResponseModel> CreateAsync(VacancyRequestModel vacancyRequestModel);
    Task UpdateAsync(int id, VacancyRequestModel vacancyRequestModel);
    Task DeleteAsync(int id);
}

