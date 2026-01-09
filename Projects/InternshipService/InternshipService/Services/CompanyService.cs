using AutoMapper;
using InternshipService.Auth;
using InternshipService.Auth.models;
using InternshipService.Models.DTO;
using InternshipService.Models.Entities;
using InternshipService.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace InternshipService.Services;

public class CompanyService(ICompanyRepository companyRepository, IMapper mapper, ICurrentUser currentUser) : ICompanyService
{
    public async Task<List<CompanyResponseModel>> GetAsync()
    {
        if (!currentUser.UserPermissions.Contains(Permissions.Read))
        {
            throw new SecurityTokenException("Authorization failed");
        }
        
        var companies = await companyRepository.GetAllAsync();
        return mapper.Map<List<CompanyResponseModel>>(companies);
    }

    public async Task<CompanyResponseModel?> GetByIdAsync(int id)
    {
        if (!currentUser.UserPermissions.Contains(Permissions.Read))
        {
            throw new SecurityTokenException("Authorization failed");
        }
        
        var company = await companyRepository.GetByIdAsync(id);
        return company is not null ? mapper.Map<CompanyResponseModel>(company) : null;
    }

    public async Task<CompanyResponseModel> CreateAsync(CompanyRequestModel companyRequestModel)
    {
        if (!currentUser.UserPermissions.Contains(Permissions.Create))
        {
            throw new SecurityTokenException("Authorization failed");
        }
        
        var company = mapper.Map<Company>(companyRequestModel);
        company.CreatedDate = DateTime.UtcNow;
        await companyRepository.CreateAsync(company);
        return mapper.Map<CompanyResponseModel>(company);
    }

    public async Task UpdateAsync(int id, CompanyRequestModel companyRequestModel)
    {
        if (!currentUser.UserPermissions.Contains(Permissions.Update))
        {
            throw new SecurityTokenException("Authorization failed");
        }
        
        if (await companyRepository.GetByIdAsync(id) is Company company)
        {
            var originalCreatedDate = company.CreatedDate;
            if (originalCreatedDate.Kind != DateTimeKind.Utc)
            {
                originalCreatedDate = originalCreatedDate.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(originalCreatedDate, DateTimeKind.Utc)
                    : originalCreatedDate.ToUniversalTime();
            }

            mapper.Map(companyRequestModel, company);
            
            company.CreatedDate = originalCreatedDate;
            
            await companyRepository.UpdateAsync(company);
        }
    }

    public async Task DeleteAsync(int id)
    {
        if (!currentUser.UserPermissions.Contains(Permissions.Delete))
        {
            throw new SecurityTokenException("Authorization failed");
        }
        
        if (await companyRepository.GetByIdAsync(id) is Company company)
        {
            await companyRepository.DeleteAsync(company.Id);
        }
    }
}

