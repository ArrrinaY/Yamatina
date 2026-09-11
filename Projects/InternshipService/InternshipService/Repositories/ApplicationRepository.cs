using InternshipService.Data;
using InternshipService.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace InternshipService.Repositories;

public class ApplicationRepository(AppDbContext dbContext) : Repository<Application>(dbContext), IApplicationRepository
{
    public async Task<List<Application>> GetByVacancyIdAsync(int vacancyId)
    {
        return await _dbSet
            .Include(a => a.Candidate)
            .Include(a => a.Vacancy)
            .Where(a => a.VacancyId == vacancyId)
            .ToListAsync();
    }

    public async Task<List<Application>> GetByCandidateIdAsync(int candidateId)
    {
        return await _dbSet
            .Include(a => a.Candidate)
            .Include(a => a.Vacancy)
                .ThenInclude(v => v.Company)
            .Where(a => a.CandidateId == candidateId)
            .ToListAsync();
    }
}

