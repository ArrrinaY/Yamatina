using InternshipService.Data;
using InternshipService.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace InternshipService.Repositories;

public class UserRepository(AppDbContext dbContext) : Repository<User>(dbContext), IUserRepository
{
    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }
}

