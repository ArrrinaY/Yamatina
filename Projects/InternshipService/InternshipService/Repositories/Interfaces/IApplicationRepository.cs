using InternshipService.Models.Entities;

namespace InternshipService.Repositories;

public interface IApplicationRepository : IRepository<Application>
{
    Task<List<Application>> GetByVacancyIdAsync(int vacancyId);
    Task<List<Application>> GetByCandidateIdAsync(int candidateId);
}

