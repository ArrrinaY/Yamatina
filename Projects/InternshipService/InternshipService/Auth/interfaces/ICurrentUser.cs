namespace InternshipService.Auth;

public interface ICurrentUser
{
    int UserId { get; }
    List<string> UserPermissions { get; }
}

