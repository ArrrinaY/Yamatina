using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InternshipService.Services;
using InternshipService.Models.DTO;
using FluentValidation;
using System.Net.Mime;

namespace InternshipService.Controllers;

[ApiController]
[Route("api/vacancies")]
[Produces("application/json")]
public class VacanciesController(IVacancyService vacancyService, IValidator<VacancyRequestModel> validator) : ControllerBase
{
    [Authorize(Policy = "CanRead")]
    [HttpGet(Name = "GetVacancies")]
    [ProducesResponseType<PaginatedResponse<VacancyResponseModel>>(StatusCodes.Status200OK)]
    public async Task<IResult> GetAsync([FromQuery] VacancyFilterModel filter)
    {
        var vacancies = await vacancyService.GetFilteredAsync(filter);
        return TypedResults.Ok(vacancies);
    }

    [Authorize(Policy = "CanRead")]
    [HttpGet("{id:int}", Name = "GetVacancyById")]
    [ProducesResponseType<VacancyResponseModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> GetByIdAsync([FromRoute] int id)
    {
        var vacancy = await vacancyService.GetByIdAsync(id);
        if (vacancy != null)
        {
            return TypedResults.Ok(vacancy);
        }
        return TypedResults.NotFound();
    }

    [Authorize(Policy = "CanCreate")]
    [HttpPost(Name = "CreateVacancy")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<VacancyResponseModel>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IResult> PostAsync([FromBody] VacancyRequestModel vacancy)
    {
        var validation = await validator.ValidateAsync(vacancy);
        if (!validation.IsValid)
        {
            var errors = validation
                .Errors.Select(error => new { error.PropertyName, error.ErrorMessage })
                .ToArray();
            return TypedResults.BadRequest(errors);
        }

        var result = await vacancyService.CreateAsync(vacancy);

        return TypedResults.CreatedAtRoute(
            routeName: "GetVacancyById",
            routeValues: new { id = result.Id },
            value: result
        );
    }

    [Authorize(Policy = "CanUpdate")]
    [HttpPut("{id:int}", Name = "UpdateVacancy")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> PutAsync([FromRoute] int id, [FromBody] VacancyRequestModel vacancy)
    {
        var validation = await validator.ValidateAsync(vacancy);
        if (!validation.IsValid)
        {
            var errors = validation
                .Errors.Select(error => new { error.PropertyName, error.ErrorMessage })
                .ToArray();
            return TypedResults.BadRequest(errors);
        }

        if (await vacancyService.GetByIdAsync(id) == null)
        {
            return TypedResults.NotFound();
        }

        await vacancyService.UpdateAsync(id, vacancy);
        return TypedResults.NoContent();
    }

    [Authorize(Policy = "CanDelete")]
    [HttpDelete("{id:int}", Name = "DeleteVacancy")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> DeleteAsync([FromRoute] int id)
    {
        if (await vacancyService.GetByIdAsync(id) == null)
        {
            return TypedResults.NotFound();
        }

        await vacancyService.DeleteAsync(id);
        return TypedResults.NoContent();
    }
}

