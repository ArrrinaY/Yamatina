using InternshipService.Data;
using InternshipService.Models.Entities;
using InternshipService.Repositories;
using InternshipService.Tests.Helpers;
using Xunit;

namespace InternshipService.Tests.Repositories;

public class TagRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly TagRepository _repository;

    public TagRepositoryTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext();
        _repository = new TagRepository(_context);
    }

    [Fact]
    public async Task CreateAsync_ShouldAddTagToDatabase()
    {
        var tag = new Tag
        {
            Name = "C#",
            Category = TagCategory.Technology
        };

        await _repository.CreateAsync(tag);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(tag.Id);
        Assert.NotNull(result);
        Assert.Equal("C#", result.Name);
        Assert.Equal(TagCategory.Technology, result.Category);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnTag_WhenExists()
    {
        var tag = new Tag
        {
            Name = "React",
            Category = TagCategory.Technology
        };
        await _repository.CreateAsync(tag);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(tag.Id);

        Assert.NotNull(result);
        Assert.Equal(tag.Id, result.Id);
        Assert.Equal("React", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        var result = await _repository.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllTags()
    {
        var tag1 = new Tag { Name = "C#", Category = TagCategory.Technology };
        var tag2 = new Tag { Name = "Marketing", Category = TagCategory.Business };
        
        await _repository.CreateAsync(tag1);
        await _repository.CreateAsync(tag2);
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllAsync();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateTag()
    {
        var tag = new Tag
        {
            Name = "Original Name",
            Category = TagCategory.Technology
        };
        await _repository.CreateAsync(tag);
        await _context.SaveChangesAsync();

        tag.Name = "Updated Name";
        tag.Category = TagCategory.Business;
        await _repository.UpdateAsync(tag);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(tag.Id);
        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.Name);
        Assert.Equal(TagCategory.Business, result.Category);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveTag()
    {
        var tag = new Tag
        {
            Name = "Test Tag",
            Category = TagCategory.Technology
        };
        await _repository.CreateAsync(tag);
        await _context.SaveChangesAsync();

        await _repository.DeleteAsync(tag.Id);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(tag.Id);
        Assert.Null(result);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

