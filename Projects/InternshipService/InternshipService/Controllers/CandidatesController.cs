using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InternshipService.Services;
using InternshipService.Models.DTO;
using FluentValidation;
using System.Net.Mime;

namespace InternshipService.Controllers;

[ApiController]
[Route("api/candidates")]
[Produces("application/json")]
public class CandidatesController(ICandidateService candidateService, IValidator<CandidateRequestModel> validator) : ControllerBase
{
    [Authorize(Policy = "CanRead")]
    [HttpGet(Name = "GetCandidates")]
    [ProducesResponseType<List<CandidateResponseModel>>(StatusCodes.Status200OK)]
    public async Task<IResult> GetAsync()
    {
        var candidates = await candidateService.GetAsync();
        return TypedResults.Ok(candidates);
    }

    [Authorize(Policy = "CanRead")]
    [HttpGet("{id:int}", Name = "GetCandidateById")]
    [ProducesResponseType<CandidateResponseModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> GetByIdAsync([FromRoute] int id)
    {
        var candidate = await candidateService.GetByIdAsync(id);
        if (candidate != null)
        {
            return TypedResults.Ok(candidate);
        }
        return TypedResults.NotFound();
    }

    [Authorize(Policy = "CanCreate")]
    [HttpPost(Name = "CreateCandidate")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<CandidateResponseModel>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IResult> PostAsync([FromBody] CandidateRequestModel candidate)
    {
        var validation = await validator.ValidateAsync(candidate);
        if (!validation.IsValid)
        {
            var errors = validation
                .Errors.Select(error => new { error.PropertyName, error.ErrorMessage })
                .ToArray();
            return TypedResults.BadRequest(errors);
        }

        var result = await candidateService.CreateAsync(candidate);

        return TypedResults.CreatedAtRoute(
            routeName: "GetCandidateById",
            routeValues: new { id = result.Id },
            value: result
        );
    }

    [Authorize(Policy = "CanUpdate")]
    [HttpPut("{id:int}", Name = "UpdateCandidate")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> PutAsync([FromRoute] int id, [FromBody] CandidateRequestModel candidate)
    {
        var validation = await validator.ValidateAsync(candidate);
        if (!validation.IsValid)
        {
            var errors = validation
                .Errors.Select(error => new { error.PropertyName, error.ErrorMessage })
                .ToArray();
            return TypedResults.BadRequest(errors);
        }

        if (await candidateService.GetByIdAsync(id) == null)
        {
            return TypedResults.NotFound();
        }

        await candidateService.UpdateAsync(id, candidate);
        return TypedResults.NoContent();
    }

    [Authorize(Policy = "CanDelete")]
    [HttpDelete("{id:int}", Name = "DeleteCandidate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> DeleteAsync([FromRoute] int id)
    {
        if (await candidateService.GetByIdAsync(id) == null)
        {
            return TypedResults.NotFound();
        }

        await candidateService.DeleteAsync(id);
        return TypedResults.NoContent();
    }
}

