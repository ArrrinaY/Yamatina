namespace InternshipService.Models.Entities;

public class ApiKey
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public bool IsActive { get; set; }
}

