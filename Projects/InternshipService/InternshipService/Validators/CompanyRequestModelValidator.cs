using FluentValidation;
using InternshipService.Models.DTO;

namespace InternshipService.Validators;

public class CompanyRequestModelValidator : AbstractValidator<CompanyRequestModel>
{
    public CompanyRequestModelValidator()
    {
        RuleFor(company => company.Name)
            .NotEmpty().WithMessage("Company name is required.")
            .MaximumLength(200).WithMessage("Company name must not exceed 200 characters.");

        RuleFor(company => company.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.")
            .MaximumLength(100).WithMessage("Email must not exceed 100 characters.");

        RuleFor(company => company.Website)
            .Must(uri => string.IsNullOrEmpty(uri) || Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("Website must be a valid URL.")
            .When(company => !string.IsNullOrEmpty(company.Website));

        RuleFor(company => company.Phone)
            .MaximumLength(20).WithMessage("Phone must not exceed 20 characters.")
            .When(company => !string.IsNullOrEmpty(company.Phone));

        RuleFor(company => company.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.")
            .When(company => !string.IsNullOrEmpty(company.Description));
    }
}

