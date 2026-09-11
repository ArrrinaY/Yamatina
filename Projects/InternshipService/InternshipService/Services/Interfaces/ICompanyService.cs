using InternshipService.Models.DTO;

namespace InternshipService.Services;

public interface ICompanyService
{
    Task<List<CompanyResponseModel>> GetAsync();
    Task<CompanyResponseModel?> GetByIdAsync(int id);
    Task<CompanyResponseModel> CreateAsync(CompanyRequestModel companyRequestModel);
    Task UpdateAsync(int id, CompanyRequestModel companyRequestModel);
    Task DeleteAsync(int id);
}

