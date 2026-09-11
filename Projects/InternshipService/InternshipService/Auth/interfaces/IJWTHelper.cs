namespace InternshipService.Auth;

public interface IJWTHelper
{
    public string GenerateToken(int userId, string role);
}

