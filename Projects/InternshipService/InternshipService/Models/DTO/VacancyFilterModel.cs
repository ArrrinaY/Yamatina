namespace InternshipService.Models.DTO;

public class VacancyFilterModel
{
    public int? Type { get; set; }
    public string? Location { get; set; }
    public List<int>? TagIds { get; set; }
    public bool? IsActive { get; set; }
    public int? CompanyId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

