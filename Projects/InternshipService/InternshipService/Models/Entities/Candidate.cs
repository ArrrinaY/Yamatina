namespace InternshipService.Models.Entities;

public enum ExperienceLevel
{
    Junior,
    Middle,
    Senior
}

public class Candidate
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? ResumeUrl { get; set; }
    public string? Skills { get; set; }
    public ExperienceLevel? ExperienceLevel { get; set; }
    
    public ICollection<Application> Applications { get; set; } = new List<Application>();
}

