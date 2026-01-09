namespace InternshipService.Models.Entities;

public class Company
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Website { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    public ICollection<Vacancy> Vacancies { get; set; } = new List<Vacancy>();
}

