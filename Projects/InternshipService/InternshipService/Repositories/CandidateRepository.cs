using InternshipService.Data;
using InternshipService.Models.Entities;

namespace InternshipService.Repositories;

public class CandidateRepository(AppDbContext dbContext) : Repository<Candidate>(dbContext), ICandidateRepository
{
}

