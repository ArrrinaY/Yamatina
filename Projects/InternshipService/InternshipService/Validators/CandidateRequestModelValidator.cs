using FluentValidation;
using InternshipService.Models.DTO;

namespace InternshipService.Validators;

public class CandidateRequestModelValidator : AbstractValidator<CandidateRequestModel>
{
    public CandidateRequestModelValidator()
    {
        RuleFor(candidate => candidate.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters.");

        RuleFor(candidate => candidate.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters.");

        RuleFor(candidate => candidate.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.")
            .MaximumLength(100).WithMessage("Email must not exceed 100 characters.");

        RuleFor(candidate => candidate.Phone)
            .MaximumLength(20).WithMessage("Phone must not exceed 20 characters.")
            .When(candidate => !string.IsNullOrEmpty(candidate.Phone));

        RuleFor(candidate => candidate.ResumeUrl)
            .Must(uri => string.IsNullOrEmpty(uri) || Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("Resume URL must be a valid URL.")
            .When(candidate => !string.IsNullOrEmpty(candidate.ResumeUrl));

        RuleFor(candidate => candidate.Skills)
            .MaximumLength(1000).WithMessage("Skills must not exceed 1000 characters.")
            .When(candidate => !string.IsNullOrEmpty(candidate.Skills));

        RuleFor(candidate => candidate.ExperienceLevel)
            .InclusiveBetween(0, 2).WithMessage("Experience level must be 0 (Junior), 1 (Middle), or 2 (Senior).")
            .When(candidate => candidate.ExperienceLevel.HasValue);
    }
}

