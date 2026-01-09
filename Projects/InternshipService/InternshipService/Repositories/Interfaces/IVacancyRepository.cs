using InternshipService.Models.Entities;

namespace InternshipService.Repositories;

public interface IVacancyRepository : IRepository<Vacancy>
{
    Task AddTagsToVacancyAsync(int vacancyId, List<int> tagIds);
    Task<List<Vacancy>> GetFilteredAsync(int? type, string? location, List<int>? tagIds, bool? isActive, int? companyId, int page, int pageSize);
    Task<int> GetFilteredCountAsync(int? type, string? location, List<int>? tagIds, bool? isActive, int? companyId);
}

