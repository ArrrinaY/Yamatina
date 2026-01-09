namespace InternshipService.Models.DTO;

public class LoginResponseModel
{
    public string Token { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int UserId { get; set; }
}

