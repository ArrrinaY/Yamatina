using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InternshipService.Services;
using InternshipService.Models.DTO;
using FluentValidation;
using System.Net.Mime;

namespace InternshipService.Controllers;

[ApiController]
[Route("api/applications")]
[Produces("application/json")]
public class ApplicationsController(IApplicationService applicationService, IValidator<ApplicationRequestModel> validator) : ControllerBase
{
    [Authorize(Policy = "CanRead")]
    [HttpGet(Name = "GetApplications")]
    [ProducesResponseType<List<ApplicationResponseModel>>(StatusCodes.Status200OK)]
    public async Task<IResult> GetAsync()
    {
        var applications = await applicationService.GetAsync();
        return TypedResults.Ok(applications);
    }

    [Authorize(Policy = "CanRead")]
    [HttpGet("{id:int}", Name = "GetApplicationById")]
    [ProducesResponseType<ApplicationResponseModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> GetByIdAsync([FromRoute] int id)
    {
        var application = await applicationService.GetByIdAsync(id);
        if (application != null)
        {
            return TypedResults.Ok(application);
        }
        return TypedResults.NotFound();
    }

    [Authorize(Policy = "CanRead")]
    [HttpGet("vacancy/{vacancyId:int}", Name = "GetApplicationsByVacancyId")]
    [ProducesResponseType<List<ApplicationResponseModel>>(StatusCodes.Status200OK)]
    public async Task<IResult> GetByVacancyIdAsync([FromRoute] int vacancyId)
    {
        var applications = await applicationService.GetByVacancyIdAsync(vacancyId);
        return TypedResults.Ok(applications);
    }

    [Authorize(Policy = "CanRead")]
    [HttpGet("candidate/{candidateId:int}", Name = "GetApplicationsByCandidateId")]
    [ProducesResponseType<List<ApplicationResponseModel>>(StatusCodes.Status200OK)]
    public async Task<IResult> GetByCandidateIdAsync([FromRoute] int candidateId)
    {
        var applications = await applicationService.GetByCandidateIdAsync(candidateId);
        return TypedResults.Ok(applications);
    }

    [Authorize(Policy = "CanCreate")]
    [HttpPost(Name = "CreateApplication")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ApplicationResponseModel>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IResult> PostAsync([FromBody] ApplicationRequestModel application)
    {
        var validation = await validator.ValidateAsync(application);
        if (!validation.IsValid)
        {
            var errors = validation
                .Errors.Select(error => new { error.PropertyName, error.ErrorMessage })
                .ToArray();
            return TypedResults.BadRequest(errors);
        }

        var result = await applicationService.CreateAsync(application);

        return TypedResults.CreatedAtRoute(
            routeName: "GetApplicationById",
            routeValues: new { id = result.Id },
            value: result
        );
    }

    [Authorize(Policy = "CanUpdate")]
    [HttpPut("{id:int}/status", Name = "UpdateApplicationStatus")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> UpdateStatusAsync([FromRoute] int id, [FromBody] ApplicationStatusUpdateModel statusUpdate)
    {
        if (await applicationService.GetByIdAsync(id) == null)
        {
            return TypedResults.NotFound();
        }

        await applicationService.UpdateStatusAsync(id, statusUpdate.Status);
        return TypedResults.NoContent();
    }

    [Authorize(Policy = "CanDelete")]
    [HttpDelete("{id:int}", Name = "DeleteApplication")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> DeleteAsync([FromRoute] int id)
    {
        if (await applicationService.GetByIdAsync(id) == null)
        {
            return TypedResults.NotFound();
        }

        await applicationService.DeleteAsync(id);
        return TypedResults.NoContent();
    }
}

