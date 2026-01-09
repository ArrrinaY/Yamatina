using InternshipService.Models.DTO;

namespace InternshipService.Services;

public interface IAuthService
{
    Task<LoginResponseModel> LoginAsync(LoginRequestModel loginRequest);
}

