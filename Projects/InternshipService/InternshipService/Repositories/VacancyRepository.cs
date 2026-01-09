using InternshipService.Data;
using InternshipService.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using Dapper;

namespace InternshipService.Repositories;

public class VacancyRepository(AppDbContext dbContext, DbConnection connection) : Repository<Vacancy>(dbContext), IVacancyRepository
{
    private readonly DbConnection _connection = connection;

    public async Task AddTagsToVacancyAsync(int vacancyId, List<int> tagIds)
    {
        if (tagIds == null || tagIds.Count == 0)
            return;

        if (_connection.State != System.Data.ConnectionState.Open)
            await _connection.OpenAsync();
            
        await using var transaction = await _connection.BeginTransactionAsync();
        
        try
        {
            await _connection.ExecuteAsync(
                "DELETE FROM vacancy_tags WHERE vacancy_id = @VacancyId",
                new { VacancyId = vacancyId },
                transaction
            );

            foreach (var tagId in tagIds)
            {
                await _connection.ExecuteAsync(
                    "INSERT INTO vacancy_tags (vacancy_id, tag_id) VALUES (@VacancyId, @TagId)",
                    new { VacancyId = vacancyId, TagId = tagId },
                    transaction
                );
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<Vacancy>> GetFilteredAsync(int? type, string? location, List<int>? tagIds, bool? isActive, int? companyId, int page, int pageSize)
    {
        var query = _dbSet
            .Include(v => v.Company)
            .Include(v => v.VacancyTags)
                .ThenInclude(vt => vt.Tag)
            .AsQueryable();

        if (type.HasValue)
            query = query.Where(v => (int)v.Type == type.Value);

        if (!string.IsNullOrWhiteSpace(location))
            query = query.Where(v => v.Location != null && v.Location.Contains(location));

        if (isActive.HasValue)
            query = query.Where(v => v.IsActive == isActive.Value);

        if (companyId.HasValue)
            query = query.Where(v => v.CompanyId == companyId.Value);

        if (tagIds != null && tagIds.Count > 0)
        {
            query = query.Where(v => v.VacancyTags.Any(vt => tagIds.Contains(vt.TagId)));
        }

        return await query
            .OrderByDescending(v => v.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetFilteredCountAsync(int? type, string? location, List<int>? tagIds, bool? isActive, int? companyId)
    {
        var query = _dbSet.AsQueryable();

        if (type.HasValue)
            query = query.Where(v => (int)v.Type == type.Value);

        if (!string.IsNullOrWhiteSpace(location))
            query = query.Where(v => v.Location != null && v.Location.Contains(location));

        if (isActive.HasValue)
            query = query.Where(v => v.IsActive == isActive.Value);

        if (companyId.HasValue)
            query = query.Where(v => v.CompanyId == companyId.Value);

        if (tagIds != null && tagIds.Count > 0)
        {
            query = query.Where(v => v.VacancyTags.Any(vt => tagIds.Contains(vt.TagId)));
        }

        return await query.CountAsync();
    }
}

