using AutoMapper;
using InternshipService.Auth;
using InternshipService.Auth.models;
using InternshipService.Models.DTO;
using InternshipService.Models.Entities;
using InternshipService.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace InternshipService.Services;

public class TagService(ITagRepository tagRepository, IMapper mapper, ICurrentUser currentUser) : ITagService
{
    public async Task<List<TagResponseModel>> GetAsync()
    {
        if (!currentUser.UserPermissions.Contains(Permissions.Read))
        {
            throw new SecurityTokenException("Authorization failed");
        }
        
        var tags = await tagRepository.GetAllAsync();
        return mapper.Map<List<TagResponseModel>>(tags);
    }

    public async Task<TagResponseModel?> GetByIdAsync(int id)
    {
        if (!currentUser.UserPermissions.Contains(Permissions.Read))
        {
            throw new SecurityTokenException("Authorization failed");
        }
        
        var tag = await tagRepository.GetByIdAsync(id);
        return tag is not null ? mapper.Map<TagResponseModel>(tag) : null;
    }

    public async Task<List<TagResponseModel>> GetByCategoryAsync(int category)
    {
        if (!currentUser.UserPermissions.Contains(Permissions.Read))
        {
            throw new SecurityTokenException("Authorization failed");
        }
        
        var allTags = await tagRepository.GetAllAsync();
        var tags = allTags.Where(t => (int)t.Category == category).ToList();
        return mapper.Map<List<TagResponseModel>>(tags);
    }

    public async Task<TagResponseModel> CreateAsync(TagRequestModel tagRequestModel)
    {
        if (!currentUser.UserPermissions.Contains(Permissions.Create))
        {
            throw new SecurityTokenException("Authorization failed");
        }
        
        var allTags = await tagRepository.GetAllAsync();
        if (allTags.Any(t => t.Name.Equals(tagRequestModel.Name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Tag with name '{tagRequestModel.Name}' already exists.");
        }

        var tag = mapper.Map<Tag>(tagRequestModel);
        await tagRepository.CreateAsync(tag);
        return mapper.Map<TagResponseModel>(tag);
    }

    public async Task UpdateAsync(int id, TagRequestModel tagRequestModel)
    {
        if (!currentUser.UserPermissions.Contains(Permissions.Update))
        {
            throw new SecurityTokenException("Authorization failed");
        }
        
        if (await tagRepository.GetByIdAsync(id) is Tag tag)
        {
            var allTags = await tagRepository.GetAllAsync();
            if (allTags.Any(t => t.Id != id && t.Name.Equals(tagRequestModel.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"Tag with name '{tagRequestModel.Name}' already exists.");
            }

            mapper.Map(tagRequestModel, tag);
            await tagRepository.UpdateAsync(tag);
        }
    }

    public async Task DeleteAsync(int id)
    {
        if (!currentUser.UserPermissions.Contains(Permissions.Delete))
        {
            throw new SecurityTokenException("Authorization failed");
        }
        
        if (await tagRepository.GetByIdAsync(id) is Tag tag)
        {
            await tagRepository.DeleteAsync(tag.Id);
        }
    }
}

