namespace InternshipService.Models.DTO;

public class VacancyRequestModel
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Requirements { get; set; }
    public string? SalaryRange { get; set; }
    public int Type { get; set; }
    public string? Location { get; set; }
    public bool IsActive { get; set; } = true;
    public int CompanyId { get; set; }
    public List<int> TagIds { get; set; } = new();
}

