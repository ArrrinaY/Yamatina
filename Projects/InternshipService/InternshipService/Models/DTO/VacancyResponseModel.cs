namespace InternshipService.Models.DTO;

public class VacancyResponseModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Requirements { get; set; }
    public string? SalaryRange { get; set; }
    public int Type { get; set; }
    public string? Location { get; set; }
    public bool IsActive { get; set; }
    public string CreatedDate { get; set; } = string.Empty;
    public int CompanyId { get; set; }
    public CompanyResponseModel? Company { get; set; }
    public List<TagResponseModel> Tags { get; set; } = new();
}

