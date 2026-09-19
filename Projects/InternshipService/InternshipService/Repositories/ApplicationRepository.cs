using System.Data.Common;
using Dapper;
using InternshipService.Data;
using InternshipService.Models.DTO;
using InternshipService.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace InternshipService.Repositories;

public class ApplicationRepository(AppDbContext dbContext, DbConnection connection) : Repository<Application>(dbContext), IApplicationRepository
{
    private readonly DbConnection _connection = connection;

    public override async ValueTask<Application?> GetByIdAsync(int id) =>
        await _dbSet.FirstOrDefaultAsync(a => a.Id == id);

    public async Task<List<Application>> GetByVacancyIdAsync(int vacancyId)
    {
        const string sql = """
            SELECT a.id, a.candidate_id, a.vacancy_id, a.status, a.applied_date, a.cover_letter
            FROM applications a
            INNER JOIN candidates c ON a.candidate_id = c.id
            INNER JOIN vacancies v ON a.vacancy_id = v.id
            WHERE a.vacancy_id = @VacancyId
            ORDER BY a.applied_date DESC
            """;

        await EnsureConnectionOpenAsync();
        var rows = await _connection.QueryAsync<Application>(sql, new { VacancyId = vacancyId });
        return rows.ToList();
    }

    public async Task<List<Application>> GetByCandidateIdAsync(int candidateId)
    {
        const string sql = """
            SELECT a.id, a.candidate_id, a.vacancy_id, a.status, a.applied_date, a.cover_letter
            FROM applications a
            INNER JOIN vacancies v ON a.vacancy_id = v.id
            INNER JOIN companies co ON v.company_id = co.id
            WHERE a.candidate_id = @CandidateId
            ORDER BY a.applied_date DESC
            """;

        await EnsureConnectionOpenAsync();
        var rows = await _connection.QueryAsync<Application>(sql, new { CandidateId = candidateId });
        return rows.ToList();
    }

    public async Task<List<Application>> GetFilteredAsync(int candidateId, ApplicationFilterModel filter)
    {
        const string sql = """
            SELECT a.id, a.candidate_id, a.vacancy_id, a.status, a.applied_date, a.cover_letter
            FROM applications a
            INNER JOIN vacancies v ON a.vacancy_id = v.id
            WHERE a.candidate_id = @CandidateId
              AND (@Status IS NULL OR a.status = @Status)
              AND (@From IS NULL OR a.applied_date >= @From)
              AND (@To IS NULL OR a.applied_date < @To)
            ORDER BY a.applied_date DESC
            LIMIT @PageSize OFFSET @Offset
            """;

        await EnsureConnectionOpenAsync();
        var rows = await _connection.QueryAsync<Application>(sql, new
        {
            CandidateId = candidateId,
            filter.Status,
            filter.From,
            filter.To,
            filter.PageSize,
            Offset = (filter.Page - 1) * filter.PageSize
        });
        return rows.ToList();
    }

    public async Task<List<ApplicationStatisticsModel>> GetStatisticsByVacancyAsync(int vacancyId)
    {
        const string sql = """
            SELECT
                a.status AS Status,
                COUNT(*) AS Count
            FROM applications a
            INNER JOIN vacancies v ON a.vacancy_id = v.id
            INNER JOIN companies c ON v.company_id = c.id
            WHERE a.vacancy_id = @VacancyId
            GROUP BY a.status
            ORDER BY a.status
            """;

        await EnsureConnectionOpenAsync();
        var rows = (await _connection.QueryAsync<ApplicationStatisticsModel>(sql, new { VacancyId = vacancyId })).ToList();

        foreach (var row in rows)
        {
            row.StatusName = Enum.IsDefined(typeof(ApplicationStatus), row.Status)
                ? ((ApplicationStatus)row.Status).ToString()
                : row.Status.ToString();
        }

        return rows;
    }

    private async Task EnsureConnectionOpenAsync()
    {
        if (_connection.State != System.Data.ConnectionState.Open)
            await _connection.OpenAsync();
    }
}
