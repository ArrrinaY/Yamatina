using FluentValidation;
using InternshipService.Models.DTO;
using InternshipService.Services;
using Microsoft.AspNetCore.Mvc;

namespace InternshipService.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController(
    IAuthService authService,
    IValidator<LoginRequestModel> loginValidator
) : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType<LoginResponseModel>(StatusCodes.Status200OK)]
    [ProducesResponseType<ExceptionModel>(StatusCodes.Status400BadRequest)]
    public async Task<IResult> LoginAsync([FromBody] LoginRequestModel request)
    {
        var validation = await loginValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        var result = await authService.LoginAsync(request);
        return TypedResults.Ok(result);
    }
}

