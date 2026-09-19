using InternshipService.Models.DTO;
using InternshipService.Models.Entities;

namespace InternshipService.Repositories;

public interface IApplicationRepository : IRepository<Application>
{
    Task<List<Application>> GetByVacancyIdAsync(int vacancyId);
    Task<List<Application>> GetByCandidateIdAsync(int candidateId);
    Task<List<Application>> GetFilteredAsync(int candidateId, ApplicationFilterModel filter);
    Task<List<ApplicationStatisticsModel>> GetStatisticsByVacancyAsync(int vacancyId);
}

