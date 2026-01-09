using FluentValidation;
using InternshipService.Models.DTO;

namespace InternshipService.Validators;

public class TagRequestModelValidator : AbstractValidator<TagRequestModel>
{
    public TagRequestModelValidator()
    {
        RuleFor(tag => tag.Name)
            .NotEmpty().WithMessage("Tag name is required.")
            .MaximumLength(100).WithMessage("Tag name must not exceed 100 characters.");

        RuleFor(tag => tag.Category)
            .InclusiveBetween(0, 2).WithMessage("Category must be 0 (Technology), 1 (Business), or 2 (Design).");
    }
}

