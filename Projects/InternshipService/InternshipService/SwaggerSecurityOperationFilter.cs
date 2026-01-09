using InternshipService.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace InternshipService;

public class SwaggerSecurityOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAuthorize =
            context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any()
            || context
                .MethodInfo.DeclaringType?.GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>()
                .Any() == true;

        if (!hasAuthorize)
            return;

        if (operation is OpenApiOperation openApiOperation)
        {
            openApiOperation.Security ??= new List<OpenApiSecurityRequirement>();
            openApiOperation.Security.Add(
                new OpenApiSecurityRequirement
                {
                    { 
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        }
                        , new List<string>()
                    }
                }
            );
            var schema = context.SchemaGenerator.GenerateSchema(typeof(ExceptionModel), context.SchemaRepository);
            openApiOperation.Responses.Add(
                "401", 
                new OpenApiResponse
                {
                    Description = "Unauthorized",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new OpenApiMediaType
                        {
                            Schema = schema
                        }
                    }
                }
            );
            openApiOperation.Responses.Add(
                "403", 
                new OpenApiResponse
                {
                    Description = "Forbidden",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new OpenApiMediaType
                        {
                            Schema = schema
                        }
                    }
                }
            );
        }
    }
}

