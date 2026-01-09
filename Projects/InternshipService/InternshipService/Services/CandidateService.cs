using AutoMapper;
using InternshipService.Auth;
using InternshipService.Auth.models;
using InternshipService.Models.DTO;
using InternshipService.Models.Entities;
using InternshipService.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace InternshipService.Services;

public class CandidateService(ICandidateRepository candidateRepository, IMapper mapper, ICurrentUser currentUser) : ICandidateService
{
    public async Task<List<CandidateResponseModel>> GetAsync()
    {
        if (!currentUser.UserPermissions.Contains(Permissions.Read))
        {
            throw new SecurityTokenException("Authorization failed");
        }
        
        var candidates = await candidateRepository.GetAllAsync();
        return mapper.Map<List<CandidateResponseModel>>(candidates);
    }

    public async Task<CandidateResponseModel?> GetByIdAsync(int id)
    {
        if (!currentUser.UserPermissions.Contains(Permissions.Read))
        {
            throw new SecurityTokenException("Authorization failed");
        }
        
        var candidate = await candidateRepository.GetByIdAsync(id);
        return candidate is not null ? mapper.Map<CandidateResponseModel>(candidate) : null;
    }

    public async Task<CandidateResponseModel> CreateAsync(CandidateRequestModel candidateRequestModel)
    {
        if (!currentUser.UserPermissions.Contains(Permissions.Create))
        {
            throw new SecurityTokenException("Authorization failed");
        }
        
        var candidate = mapper.Map<Candidate>(candidateRequestModel);
        await candidateRepository.CreateAsync(candidate);
        return mapper.Map<CandidateResponseModel>(candidate);
    }

    public async Task UpdateAsync(int id, CandidateRequestModel candidateRequestModel)
    {
        if (!currentUser.UserPermissions.Contains(Permissions.Update))
        {
            throw new SecurityTokenException("Authorization failed");
        }
        
        if (await candidateRepository.GetByIdAsync(id) is Candidate candidate)
        {
            mapper.Map(candidateRequestModel, candidate);
            await candidateRepository.UpdateAsync(candidate);
        }
    }

    public async Task DeleteAsync(int id)
    {
        if (!currentUser.UserPermissions.Contains(Permissions.Delete))
        {
            throw new SecurityTokenException("Authorization failed");
        }
        
        if (await candidateRepository.GetByIdAsync(id) is Candidate candidate)
        {
            await candidateRepository.DeleteAsync(candidate.Id);
        }
    }
}

