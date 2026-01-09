namespace InternshipService.Models.DTO;

public class CompanyRequestModel
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Website { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
}

