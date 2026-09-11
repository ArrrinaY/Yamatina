using FluentValidation;
using InternshipService.Models.DTO;

namespace InternshipService.Validators;

public class VacancyRequestModelValidator : AbstractValidator<VacancyRequestModel>
{
    public VacancyRequestModelValidator()
    {
        RuleFor(vacancy => vacancy.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(vacancy => vacancy.Description)
            .MaximumLength(5000).WithMessage("Description must not exceed 5000 characters.")
            .When(vacancy => !string.IsNullOrEmpty(vacancy.Description));

        RuleFor(vacancy => vacancy.Requirements)
            .MaximumLength(5000).WithMessage("Requirements must not exceed 5000 characters.")
            .When(vacancy => !string.IsNullOrEmpty(vacancy.Requirements));

        RuleFor(vacancy => vacancy.SalaryRange)
            .MaximumLength(100).WithMessage("Salary range must not exceed 100 characters.")
            .When(vacancy => !string.IsNullOrEmpty(vacancy.SalaryRange));

        RuleFor(vacancy => vacancy.Type)
            .InclusiveBetween(0, 1).WithMessage("Type must be 0 (FullTime) or 1 (Internship).");

        RuleFor(vacancy => vacancy.Location)
            .MaximumLength(200).WithMessage("Location must not exceed 200 characters.")
            .When(vacancy => !string.IsNullOrEmpty(vacancy.Location));

        RuleFor(vacancy => vacancy.CompanyId)
            .GreaterThan(0).WithMessage("Company ID is required and must be greater than 0.");
    }
}

