using FluentValidation;
using InternshipService.Models.DTO;

namespace InternshipService.Validators;

public class LoginRequestModelValidator : AbstractValidator<LoginRequestModel>
{
    public LoginRequestModelValidator()
    {
        RuleFor(user => user.Username).NotEmpty().WithMessage("Username is required");
        RuleFor(user => user.Password).NotEmpty().WithMessage("Password is required");
    }
}

