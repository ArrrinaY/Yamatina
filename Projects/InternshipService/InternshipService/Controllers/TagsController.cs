using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InternshipService.Services;
using InternshipService.Models.DTO;
using FluentValidation;
using System.Net.Mime;

namespace InternshipService.Controllers;

[ApiController]
[Route("api/tags")]
[Produces("application/json")]
public class TagsController(ITagService tagService, IValidator<TagRequestModel> validator) : ControllerBase
{
    [Authorize(Policy = "CanRead")]
    [HttpGet(Name = "GetTags")]
    [ProducesResponseType<List<TagResponseModel>>(StatusCodes.Status200OK)]
    public async Task<IResult> GetAsync()
    {
        var tags = await tagService.GetAsync();
        return TypedResults.Ok(tags);
    }

    [Authorize(Policy = "CanRead")]
    [HttpGet("{id:int}", Name = "GetTagById")]
    [ProducesResponseType<TagResponseModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> GetByIdAsync([FromRoute] int id)
    {
        var tag = await tagService.GetByIdAsync(id);
        if (tag != null)
        {
            return TypedResults.Ok(tag);
        }
        return TypedResults.NotFound();
    }

    [Authorize(Policy = "CanRead")]
    [HttpGet("category/{category:int}", Name = "GetTagsByCategory")]
    [ProducesResponseType<List<TagResponseModel>>(StatusCodes.Status200OK)]
    public async Task<IResult> GetByCategoryAsync([FromRoute] int category)
    {
        var tags = await tagService.GetByCategoryAsync(category);
        return TypedResults.Ok(tags);
    }

    [Authorize(Policy = "CanCreate")]
    [HttpPost(Name = "CreateTag")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<TagResponseModel>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IResult> PostAsync([FromBody] TagRequestModel tag)
    {
        var validation = await validator.ValidateAsync(tag);
        if (!validation.IsValid)
        {
            var errors = validation
                .Errors.Select(error => new { error.PropertyName, error.ErrorMessage })
                .ToArray();
            return TypedResults.BadRequest(errors);
        }

        var result = await tagService.CreateAsync(tag);

        return TypedResults.CreatedAtRoute(
            routeName: "GetTagById",
            routeValues: new { id = result.Id },
            value: result
        );
    }

    [Authorize(Policy = "CanUpdate")]
    [HttpPut("{id:int}", Name = "UpdateTag")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> PutAsync([FromRoute] int id, [FromBody] TagRequestModel tag)
    {
        var validation = await validator.ValidateAsync(tag);
        if (!validation.IsValid)
        {
            var errors = validation
                .Errors.Select(error => new { error.PropertyName, error.ErrorMessage })
                .ToArray();
            return TypedResults.BadRequest(errors);
        }

        if (await tagService.GetByIdAsync(id) == null)
        {
            return TypedResults.NotFound();
        }

        await tagService.UpdateAsync(id, tag);
        return TypedResults.NoContent();
    }

    [Authorize(Policy = "CanDelete")]
    [HttpDelete("{id:int}", Name = "DeleteTag")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> DeleteAsync([FromRoute] int id)
    {
        if (await tagService.GetByIdAsync(id) == null)
        {
            return TypedResults.NotFound();
        }

        await tagService.DeleteAsync(id);
        return TypedResults.NoContent();
    }
}

