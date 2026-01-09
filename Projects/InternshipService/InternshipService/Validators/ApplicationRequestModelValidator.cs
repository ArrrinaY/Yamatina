using FluentValidation;
using InternshipService.Models.DTO;

namespace InternshipService.Validators;

public class ApplicationRequestModelValidator : AbstractValidator<ApplicationRequestModel>
{
    public ApplicationRequestModelValidator()
    {
        RuleFor(application => application.CandidateId)
            .GreaterThan(0).WithMessage("Candidate ID is required and must be greater than 0.");

        RuleFor(application => application.VacancyId)
            .GreaterThan(0).WithMessage("Vacancy ID is required and must be greater than 0.");

        RuleFor(application => application.CoverLetter)
            .MaximumLength(5000).WithMessage("Cover letter must not exceed 5000 characters.")
            .When(application => !string.IsNullOrEmpty(application.CoverLetter));
    }
}

