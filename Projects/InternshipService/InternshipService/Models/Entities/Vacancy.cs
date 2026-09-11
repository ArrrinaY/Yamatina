namespace InternshipService.Models.Entities;

public enum VacancyType
{
    FullTime,
    Internship
}

public class Vacancy
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Requirements { get; set; }
    public string? SalaryRange { get; set; }
    public VacancyType Type { get; set; }
    public string? Location { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public int CompanyId { get; set; }
    
    public Company Company { get; set; } = null!;
    public ICollection<Application> Applications { get; set; } = new List<Application>();
    public ICollection<VacancyTag> VacancyTags { get; set; } = new List<VacancyTag>();
}

