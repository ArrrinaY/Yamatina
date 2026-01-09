using AutoMapper;
using InternshipService.Auth;
using InternshipService.Auth.models;
using InternshipService.Models.DTO;
using InternshipService.Models.Entities;
using InternshipService.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace InternshipService.Services;

public class VacancyService(
    IVacancyRepository vacancyRepository,
    ICompanyRepository companyRepository,
    ITagRepository tagRepository,
    IMapper mapper,
    ILogger<VacancyService> logger,
    ICurrentUser currentUser,
    ICacheService? cacheService = null) : IVacancyService
{
    private const string CacheKeyPrefix = "vacancies:";
    private const int CacheExpirationMinutes = 5;

    public async Task<PaginatedResponse<VacancyResponseModel>> GetFilteredAsync(VacancyFilterModel filter)
    {
        if (!currentUser.UserPermissions.Contains(Permissions.Read))
        {
            throw new SecurityTokenException("Authorization failed");
        }
        
        try
        {
            var cacheKey = $"{CacheKeyPrefix}filter:{filter.Type}_{filter.Location}_{string.Join(",", filter.TagIds ?? new List<int>())}_{filter.IsActive}_{filter.CompanyId}_{filter.Page}_{filter.PageSize}";

            if (cacheService != null)
            {
                try
                {
                    var cached = await cacheService.GetAsync<PaginatedResponse<VacancyResponseModel>>(cacheKey);
                    if (cached != null)
                    {
                        return cached;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to get from cache, continuing with database query");
                }
            }

            var vacancies = await vacancyRepository.GetFilteredAsync(
                filter.Type,
                filter.Location,
                filter.TagIds,
                filter.IsActive,
                filter.CompanyId,
                filter.Page,
                filter.PageSize
            );

            var totalCount = await vacancyRepository.GetFilteredCountAsync(
                filter.Type,
                filter.Location,
                filter.TagIds,
                filter.IsActive,
                filter.CompanyId
            );

            var vacancyResponseModels = mapper.Map<List<VacancyResponseModel>>(vacancies);

            var response = new PaginatedResponse<VacancyResponseModel>
            {
                Items = vacancyResponseModels,
                Page = filter.Page,
                PageSize = filter.PageSize,
                Total = totalCount
            };

            if (cacheService != null)
            {
                try
                {
                    await cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(CacheExpirationMinutes));
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to set cache, continuing without caching");
                }
            }

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in GetFilteredAsync");
            throw;
        }
    }

    public async Task<VacancyResponseModel?> GetByIdAsync(int id)
    {
        if (!currentUser.UserPermissions.Contains(Permissions.Read))
        {
            throw new SecurityTokenException("Authorization failed");
        }
        
        var vacancy = await vacancyRepository.GetByIdAsync(id);
        return vacancy is not null ? mapper.Map<VacancyResponseModel>(vacancy) : null;
    }

    public async Task<VacancyResponseModel> CreateAsync(VacancyRequestModel vacancyRequestModel)
    {
        if (!currentUser.UserPermissions.Contains(Permissions.Create))
        {
            throw new SecurityTokenException("Authorization failed");
        }
        
        var company = await companyRepository.GetByIdAsync(vacancyRequestModel.CompanyId);
        if (company == null)
        {
            throw new ArgumentException($"Company with ID {vacancyRequestModel.CompanyId} does not exist.");
        }

        var tagIds = vacancyRequestModel.TagIds ?? new List<int>();
        if (tagIds.Count > 0)
        {
            var allTags = await tagRepository.GetAllAsync();
            var existingTagIds = allTags.Select(t => t.Id).ToList();
            var invalidTagIds = tagIds.Except(existingTagIds).ToList();
            
            if (invalidTagIds.Count > 0)
            {
                throw new ArgumentException($"Tags with IDs {string.Join(", ", invalidTagIds)} do not exist.");
            }
        }

        var vacancy = mapper.Map<Vacancy>(vacancyRequestModel);
        await vacancyRepository.CreateAsync(vacancy);

        if (tagIds.Count > 0)
        {
            await vacancyRepository.AddTagsToVacancyAsync(vacancy.Id, tagIds);
        }

        if (cacheService != null)
        {
            await cacheService.RemoveByPatternAsync($"{CacheKeyPrefix}*");
        }

        var createdVacancy = await vacancyRepository.GetByIdAsync(vacancy.Id);
        return mapper.Map<VacancyResponseModel>(createdVacancy);
    }

    public async Task UpdateAsync(int id, VacancyRequestModel vacancyRequestModel)
    {
        if (!currentUser.UserPermissions.Contains(Permissions.Update))
        {
            throw new SecurityTokenException("Authorization failed");
        }
        
        var vacancy = await vacancyRepository.GetByIdAsync(id);
        if (vacancy == null)
        {
            throw new KeyNotFoundException($"Vacancy with ID {id} not found");
        }
        
        var company = await companyRepository.GetByIdAsync(vacancyRequestModel.CompanyId);
        if (company == null)
        {
            throw new ArgumentException($"Company with ID {vacancyRequestModel.CompanyId} does not exist.");
        }

        var tagIds = vacancyRequestModel.TagIds ?? new List<int>();
        if (tagIds.Count > 0)
        {
            var allTags = await tagRepository.GetAllAsync();
            var existingTagIds = allTags.Select(t => t.Id).ToList();
            var invalidTagIds = tagIds.Except(existingTagIds).ToList();
            
            if (invalidTagIds.Count > 0)
            {
                throw new ArgumentException($"Tags with IDs {string.Join(", ", invalidTagIds)} do not exist.");
            }
        }

        var originalCreatedDate = vacancy.CreatedDate;
        if (originalCreatedDate.Kind != DateTimeKind.Utc)
        {
            originalCreatedDate = originalCreatedDate.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(originalCreatedDate, DateTimeKind.Utc)
                : originalCreatedDate.ToUniversalTime();
        }

        mapper.Map(vacancyRequestModel, vacancy);
        
        vacancy.CreatedDate = originalCreatedDate;
        
        await vacancyRepository.UpdateAsync(vacancy);

        await vacancyRepository.AddTagsToVacancyAsync(vacancy.Id, tagIds);

        if (cacheService != null)
        {
            await cacheService.RemoveByPatternAsync($"{CacheKeyPrefix}*");
        }
    }

    public async Task DeleteAsync(int id)
    {
        if (!currentUser.UserPermissions.Contains(Permissions.Delete))
        {
            throw new SecurityTokenException("Authorization failed");
        }
        
        if (await vacancyRepository.GetByIdAsync(id) is Vacancy vacancy)
        {
            await vacancyRepository.DeleteAsync(vacancy.Id);

            if (cacheService != null)
            {
                await cacheService.RemoveByPatternAsync($"{CacheKeyPrefix}*");
            }
        }
    }
}
