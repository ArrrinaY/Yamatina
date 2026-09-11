using InternshipService.Models.DTO;

namespace InternshipService.Services;

public interface ITagService
{
    Task<List<TagResponseModel>> GetAsync();
    Task<TagResponseModel?> GetByIdAsync(int id);
    Task<List<TagResponseModel>> GetByCategoryAsync(int category);
    Task<TagResponseModel> CreateAsync(TagRequestModel tagRequestModel);
    Task UpdateAsync(int id, TagRequestModel tagRequestModel);
    Task DeleteAsync(int id);
}

