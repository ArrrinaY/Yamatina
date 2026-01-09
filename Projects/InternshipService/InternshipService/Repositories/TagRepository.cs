using InternshipService.Data;
using InternshipService.Models.Entities;

namespace InternshipService.Repositories;

public class TagRepository(AppDbContext dbContext) : Repository<Tag>(dbContext), ITagRepository
{
}

