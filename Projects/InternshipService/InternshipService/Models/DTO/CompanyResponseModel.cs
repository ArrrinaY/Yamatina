namespace InternshipService.Models.DTO;

public class CompanyResponseModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Website { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string CreatedDate { get; set; } = string.Empty;
}

