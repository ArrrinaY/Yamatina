namespace InternshipService.Models.Entities;

public enum ApplicationStatus
{
    Applied,
    Reviewed,
    Rejected,
    Accepted
}

public class Application
{
    public int Id { get; set; }
    public int CandidateId { get; set; }
    public int VacancyId { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Applied;
    public DateTime AppliedDate { get; set; } = DateTime.UtcNow;
    public string? CoverLetter { get; set; }
    
    public Candidate Candidate { get; set; } = null!;
    public Vacancy Vacancy { get; set; } = null!;
}

