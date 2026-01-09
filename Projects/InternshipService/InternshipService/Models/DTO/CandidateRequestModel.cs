namespace InternshipService.Models.DTO;

public class CandidateRequestModel
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? ResumeUrl { get; set; }
    public string? Skills { get; set; }
    public int? ExperienceLevel { get; set; }
}

