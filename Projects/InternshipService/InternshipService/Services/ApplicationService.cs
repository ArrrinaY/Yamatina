using AutoMapper;
using InternshipService.Auth;
using InternshipService.Auth.models;
using InternshipService.Models.DTO;
using InternshipService.Models.Entities;
using InternshipService.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace InternshipService.Services;

public class ApplicationService(
    IApplicationRepository applicationRepository,
    IVacancyRepository vacancyRepository,
    ICandidateRepository candidateRepository,
    IMapper mapper,
    ICurrentUser currentUser) : IApplicationService
{
    public async Task<List<ApplicationResponseModel>> GetAsync()
    {
        if (!currentUser.UserPermissions.Contains(Permissions.Read))
        {
            throw new SecurityTokenException("Authorization failed");
        }
        
        var applications = await applicationRepository.GetAllAsync();
        return mapper.Map<List<ApplicationResponseModel>>(applications);
    }

    public async Task<ApplicationResponseModel?> GetByIdAsync(int id)
    {
        if (!currentUser.UserPermissions.Contains(Permissions.Read))
        {
            throw new SecurityTokenException("Authorization failed");
        }
        
        var application = await applicationRepository.GetByIdAsync(id);
        return application is not null ? mapper.Map<ApplicationResponseModel>(application) : null;
    }

    public async Task<List<ApplicationResponseModel>> GetByVacancyIdAsync(int vacancyId)
    {
        if (!currentUser.UserPermissions.Contains(Permissions.Read))
        {
            throw new SecurityTokenException("Authorization failed");
        }
        
        var applications = await applicationRepository.GetByVacancyIdAsync(vacancyId);
        return mapper.Map<List<ApplicationResponseModel>>(applications);
    }

    public async Task<List<ApplicationResponseModel>> GetByCandidateIdAsync(int candidateId)
    {
        if (!currentUser.UserPermissions.Contains(Permissions.Read))
        {
            throw new SecurityTokenException("Authorization failed");
        }
        
        var applications = await applicationRepository.GetByCandidateIdAsync(candidateId);
        return mapper.Map<List<ApplicationResponseModel>>(applications);
    }

    public async Task<ApplicationResponseModel> CreateAsync(ApplicationRequestModel applicationRequestModel)
    {
        if (!currentUser.UserPermissions.Contains(Permissions.Create))
        {
            throw new SecurityTokenException("Authorization failed");
        }
        
        var vacancy = await vacancyRepository.GetByIdAsync(applicationRequestModel.VacancyId);
        if (vacancy == null)
        {
            throw new ArgumentException($"Vacancy with ID {applicationRequestModel.VacancyId} does not exist.");
        }

        var candidate = await candidateRepository.GetByIdAsync(applicationRequestModel.CandidateId);
        if (candidate == null)
        {
            throw new ArgumentException($"Candidate with ID {applicationRequestModel.CandidateId} does not exist.");
        }

        var existingApplications = await applicationRepository.GetByVacancyIdAsync(applicationRequestModel.VacancyId);
        if (existingApplications.Any(a => a.CandidateId == applicationRequestModel.CandidateId))
        {
            throw new InvalidOperationException("Application for this vacancy already exists for this candidate.");
        }

        var application = mapper.Map<Application>(applicationRequestModel);
        await applicationRepository.CreateAsync(application);

        var createdApplication = await applicationRepository.GetByIdAsync(application.Id);
        return mapper.Map<ApplicationResponseModel>(createdApplication);
    }

    public async Task UpdateStatusAsync(int id, int status)
    {
        if (!currentUser.UserPermissions.Contains(Permissions.Update))
        {
            throw new SecurityTokenException("Authorization failed");
        }
        
        var application = await applicationRepository.GetByIdAsync(id);
        if (application == null)
        {
            throw new KeyNotFoundException($"Application with ID {id} not found");
        }

        if (!Enum.IsDefined(typeof(ApplicationStatus), status))
        {
            throw new ArgumentException($"Invalid status value: {status}");
        }

        var originalAppliedDate = application.AppliedDate;
        if (originalAppliedDate.Kind != DateTimeKind.Utc)
        {
            originalAppliedDate = originalAppliedDate.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(originalAppliedDate, DateTimeKind.Utc)
                : originalAppliedDate.ToUniversalTime();
        }

        application.Status = (ApplicationStatus)status;
        application.AppliedDate = originalAppliedDate;
        
        await applicationRepository.UpdateAsync(application);
    }

    public async Task DeleteAsync(int id)
    {
        if (!currentUser.UserPermissions.Contains(Permissions.Delete))
        {
            throw new SecurityTokenException("Authorization failed");
        }
        
        if (await applicationRepository.GetByIdAsync(id) is Application application)
        {
            await applicationRepository.DeleteAsync(application.Id);
        }
    }
}

