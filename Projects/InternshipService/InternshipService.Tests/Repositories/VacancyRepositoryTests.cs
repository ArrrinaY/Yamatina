using InternshipService.Data;
using InternshipService.Models.Entities;
using InternshipService.Repositories;
using InternshipService.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;
using Npgsql;
using Xunit;

namespace InternshipService.Tests.Repositories;

public class VacancyRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly VacancyRepository _repository;
    private readonly CompanyRepository _companyRepository;
    private readonly TagRepository _tagRepository;
    private readonly Mock<NpgsqlConnection> _mockConnection;

    public VacancyRepositoryTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext();
        _mockConnection = new Mock<NpgsqlConnection>();
        _repository = new VacancyRepository(_context, _mockConnection.Object);
        _companyRepository = new CompanyRepository(_context);
        _tagRepository = new TagRepository(_context);
    }

    [Fact]
    public async Task CreateAsync_ShouldAddVacancyToDatabase()
    {
        var company = new Company { Name = "Test Company", Email = "test@example.com" };
        await _companyRepository.CreateAsync(company);
        await _context.SaveChangesAsync();

        var vacancy = new Vacancy
        {
            Title = "Test Vacancy",
            Description = "Test Description",
            Requirements = "Test Requirements",
            Type = VacancyType.FullTime,
            Location = "Test Location",
            CompanyId = company.Id,
            IsActive = true
        };

        await _repository.CreateAsync(vacancy);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(vacancy.Id);
        Assert.NotNull(result);
        Assert.Equal("Test Vacancy", result.Title);
        Assert.Equal(company.Id, result.CompanyId);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnVacancy_WhenExists()
    {
        var company = new Company { Name = "Test Company", Email = "test@example.com" };
        await _companyRepository.CreateAsync(company);
        await _context.SaveChangesAsync();

        var vacancy = new Vacancy
        {
            Title = "Test Vacancy",
            Type = VacancyType.FullTime,
            CompanyId = company.Id
        };
        await _repository.CreateAsync(vacancy);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(vacancy.Id);

        Assert.NotNull(result);
        Assert.Equal(vacancy.Id, result.Id);
        Assert.Equal("Test Vacancy", result.Title);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllVacancies()
    {
        var company = new Company { Name = "Test Company", Email = "test@example.com" };
        await _companyRepository.CreateAsync(company);
        await _context.SaveChangesAsync();

        var vacancy1 = new Vacancy { Title = "Vacancy 1", Type = VacancyType.FullTime, CompanyId = company.Id };
        var vacancy2 = new Vacancy { Title = "Vacancy 2", Type = VacancyType.PartTime, CompanyId = company.Id };
        
        await _repository.CreateAsync(vacancy1);
        await _repository.CreateAsync(vacancy2);
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllAsync();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetFilteredAsync_ShouldReturnFilteredVacancies()
    {
        var company = new Company { Name = "Test Company", Email = "test@example.com" };
        await _companyRepository.CreateAsync(company);
        await _context.SaveChangesAsync();

        var vacancy1 = new Vacancy 
        { 
            Title = "FullTime Vacancy", 
            Type = VacancyType.FullTime, 
            Location = "City A",
            CompanyId = company.Id,
            IsActive = true
        };
        var vacancy2 = new Vacancy 
        { 
            Title = "PartTime Vacancy", 
            Type = VacancyType.PartTime, 
            Location = "City B",
            CompanyId = company.Id,
            IsActive = true
        };
        
        await _repository.CreateAsync(vacancy1);
        await _repository.CreateAsync(vacancy2);
        await _context.SaveChangesAsync();

        var result = await _repository.GetFilteredAsync(
            type: (int)VacancyType.FullTime,
            location: null,
            tagIds: null,
            isActive: true,
            companyId: null,
            page: 1,
            pageSize: 10
        );

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("FullTime Vacancy", result.First().Title);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateVacancy()
    {
        var company = new Company { Name = "Test Company", Email = "test@example.com" };
        await _companyRepository.CreateAsync(company);
        await _context.SaveChangesAsync();

        var vacancy = new Vacancy
        {
            Title = "Original Title",
            Type = VacancyType.FullTime,
            CompanyId = company.Id
        };
        await _repository.CreateAsync(vacancy);
        await _context.SaveChangesAsync();

        vacancy.Title = "Updated Title";
        vacancy.Location = "New Location";
        await _repository.UpdateAsync(vacancy);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(vacancy.Id);
        Assert.NotNull(result);
        Assert.Equal("Updated Title", result.Title);
        Assert.Equal("New Location", result.Location);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveVacancy()
    {
        var company = new Company { Name = "Test Company", Email = "test@example.com" };
        await _companyRepository.CreateAsync(company);
        await _context.SaveChangesAsync();

        var vacancy = new Vacancy
        {
            Title = "Test Vacancy",
            Type = VacancyType.FullTime,
            CompanyId = company.Id
        };
        await _repository.CreateAsync(vacancy);
        await _context.SaveChangesAsync();

        await _repository.DeleteAsync(vacancy.Id);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(vacancy.Id);
        Assert.Null(result);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
