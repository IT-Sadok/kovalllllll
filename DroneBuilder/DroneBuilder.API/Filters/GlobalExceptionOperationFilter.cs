using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DroneBuilder.API.Filters;

public class GlobalExceptionOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Responses.TryAdd("400", new OpenApiResponse
        {
            Description = "Bad Request - Validation Error or Invalid Request",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/problem+json"] = new()
                {
                    Schema = context.SchemaGenerator.GenerateSchema(typeof(ProblemDetails), context.SchemaRepository)
                }
            }
        });

        operation.Responses.TryAdd("401", new OpenApiResponse
        {
            Description = "Unauthorized - Missing or Invalid Token",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/problem+json"] = new()
                {
                    Schema = context.SchemaGenerator.GenerateSchema(typeof(ProblemDetails), context.SchemaRepository)
                }
            }
        });

        operation.Responses.TryAdd("403", new OpenApiResponse
        {
            Description = "Forbidden - Insufficient Permissions",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/problem+json"] = new()
                {
                    Schema = context.SchemaGenerator.GenerateSchema(typeof(ProblemDetails), context.SchemaRepository)
                }
            }
        });

        operation.Responses.TryAdd("404", new OpenApiResponse
        {
            Description = "Not Found - Resource Does Not Exist",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/problem+json"] = new()
                {
                    Schema = context.SchemaGenerator.GenerateSchema(typeof(ProblemDetails), context.SchemaRepository)
                }
            }
        });

        operation.Responses.TryAdd("500", new OpenApiResponse
        {
            Description = "Internal Server Error - Unexpected Error",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/problem+json"] = new()
                {
                    Schema = context.SchemaGenerator.GenerateSchema(typeof(ProblemDetails), context.SchemaRepository)
                }
            }
        });
    }
}

