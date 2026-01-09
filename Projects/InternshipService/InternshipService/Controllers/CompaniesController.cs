using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InternshipService.Services;
using InternshipService.Models.DTO;
using FluentValidation;
using System.Net.Mime;

namespace InternshipService.Controllers;

[ApiController]
[Route("api/companies")]
[Produces("application/json")]
public class CompaniesController(ICompanyService companyService, IValidator<CompanyRequestModel> validator) : ControllerBase
{
    [Authorize(Policy = "CanRead")]
    [HttpGet(Name = "GetCompanies")]
    [ProducesResponseType<List<CompanyResponseModel>>(StatusCodes.Status200OK)]
    public async Task<IResult> GetAsync()
    {
        var companies = await companyService.GetAsync();
        return TypedResults.Ok(companies);
    }

    [Authorize(Policy = "CanRead")]
    [HttpGet("{id:int}", Name = "GetCompanyById")]
    [ProducesResponseType<CompanyResponseModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> GetByIdAsync([FromRoute] int id)
    {
        var company = await companyService.GetByIdAsync(id);
        if (company != null)
        {
            return TypedResults.Ok(company);
        }
        return TypedResults.NotFound();
    }

    [Authorize(Policy = "CanCreate")]
    [HttpPost(Name = "CreateCompany")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<CompanyResponseModel>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IResult> PostAsync([FromBody] CompanyRequestModel company)
    {
        var validation = await validator.ValidateAsync(company);
        if (!validation.IsValid)
        {
            var errors = validation
                .Errors.Select(error => new { error.PropertyName, error.ErrorMessage })
                .ToArray();
            return TypedResults.BadRequest(errors);
        }

        var result = await companyService.CreateAsync(company);

        return TypedResults.CreatedAtRoute(
            routeName: "GetCompanyById",
            routeValues: new { id = result.Id },
            value: result
        );
    }

    [Authorize(Policy = "CanUpdate")]
    [HttpPut("{id:int}", Name = "UpdateCompany")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> PutAsync([FromRoute] int id, [FromBody] CompanyRequestModel company)
    {
        var validation = await validator.ValidateAsync(company);
        if (!validation.IsValid)
        {
            var errors = validation
                .Errors.Select(error => new { error.PropertyName, error.ErrorMessage })
                .ToArray();
            return TypedResults.BadRequest(errors);
        }

        if (await companyService.GetByIdAsync(id) == null)
        {
            return TypedResults.NotFound();
        }

        await companyService.UpdateAsync(id, company);
        return TypedResults.NoContent();
    }

    [Authorize(Policy = "CanDelete")]
    [HttpDelete("{id:int}", Name = "DeleteCompany")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> DeleteAsync([FromRoute] int id)
    {
        if (await companyService.GetByIdAsync(id) == null)
        {
            return TypedResults.NotFound();
        }

        await companyService.DeleteAsync(id);
        return TypedResults.NoContent();
    }
}

