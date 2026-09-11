using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace InternshipService.Repositories;

public class Repository<T>(DbContext dbContext) : IRepository<T> where T : class
{
    protected readonly DbSet<T> _dbSet = dbContext.Set<T>();
    protected readonly DbContext _dbContext = dbContext;
    
    public async Task<List<T>> GetAllAsync() => await _dbSet.ToListAsync();
    
    public async ValueTask<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);
    
    public async Task CreateAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _dbContext.SaveChangesAsync();
    }
    
    public async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _dbContext.SaveChangesAsync();
    }
    
    public async Task DeleteAsync(int id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }
    }
}

