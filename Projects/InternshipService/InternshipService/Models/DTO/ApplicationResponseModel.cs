namespace InternshipService.Models.DTO;

public class ApplicationResponseModel
{
    public int Id { get; set; }
    public int CandidateId { get; set; }
    public int VacancyId { get; set; }
    public int Status { get; set; }
    public string AppliedDate { get; set; } = string.Empty;
    public string? CoverLetter { get; set; }
    public CandidateResponseModel? Candidate { get; set; }
    public VacancyResponseModel? Vacancy { get; set; }
}

