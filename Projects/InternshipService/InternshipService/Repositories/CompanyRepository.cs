using InternshipService.Data;
using InternshipService.Models.Entities;

namespace InternshipService.Repositories;

public class CompanyRepository(AppDbContext dbContext) : Repository<Company>(dbContext), ICompanyRepository
{
}

