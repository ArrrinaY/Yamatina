using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace InternshipService.Middleware;

public class ValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .SelectMany(kvp => kvp.Value?.Errors.Select(error =>
                    new ValidationFailure(kvp.Key, error.ErrorMessage ?? string.Empty)
                ) ?? Enumerable.Empty<ValidationFailure>());
            throw new ValidationException(errors);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        
    }
}

