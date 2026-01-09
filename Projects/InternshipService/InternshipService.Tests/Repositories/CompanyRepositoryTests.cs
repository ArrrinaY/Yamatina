using InternshipService.Data;
using InternshipService.Models.Entities;
using InternshipService.Repositories;
using InternshipService.Tests.Helpers;
using Xunit;

namespace InternshipService.Tests.Repositories;

public class CompanyRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly CompanyRepository _repository;

    public CompanyRepositoryTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext();
        _repository = new CompanyRepository(_context);
    }

    [Fact]
    public async Task CreateAsync_ShouldAddCompanyToDatabase()
    {
        var company = new Company
        {
            Name = "Test Company",
            Description = "Test Description",
            Email = "test@example.com",
            Website = "https://example.com",
            Phone = "1234567890"
        };

        await _repository.CreateAsync(company);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(company.Id);
        Assert.NotNull(result);
        Assert.Equal("Test Company", result.Name);
        Assert.Equal("test@example.com", result.Email);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCompany_WhenExists()
    {
        var company = new Company
        {
            Name = "Test Company",
            Email = "test@example.com"
        };
        await _repository.CreateAsync(company);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(company.Id);

        Assert.NotNull(result);
        Assert.Equal(company.Id, result.Id);
        Assert.Equal("Test Company", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        var result = await _repository.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllCompanies()
    {
        var company1 = new Company { Name = "Company 1", Email = "company1@example.com" };
        var company2 = new Company { Name = "Company 2", Email = "company2@example.com" };
        
        await _repository.CreateAsync(company1);
        await _repository.CreateAsync(company2);
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllAsync();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateCompany()
    {
        var company = new Company
        {
            Name = "Original Name",
            Email = "original@example.com"
        };
        await _repository.CreateAsync(company);
        await _context.SaveChangesAsync();

        company.Name = "Updated Name";
        company.Email = "updated@example.com";
        await _repository.UpdateAsync(company);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(company.Id);
        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.Name);
        Assert.Equal("updated@example.com", result.Email);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveCompany()
    {
        var company = new Company
        {
            Name = "Test Company",
            Email = "test@example.com"
        };
        await _repository.CreateAsync(company);
        await _context.SaveChangesAsync();

        await _repository.DeleteAsync(company.Id);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(company.Id);
        Assert.Null(result);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

