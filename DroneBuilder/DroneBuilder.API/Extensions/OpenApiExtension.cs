using DroneBuilder.API.Documentation;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;

namespace DroneBuilder.API.Extensions;

public static class OpenApiExtension
{
    public static IServiceCollection AddOpenApiConfig(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Info = new OpenApiInfo
                {
                    Title = "DroneBuilder API",
                    Version = "v1",
                    Description = "API for managing drone products, configurations, orders and warehouse operations.",
                    Contact = new OpenApiContact
                    {
                        Name = "DroneBuilder Dev Team",
                        Email = "dev@dronebuilder.io"
                    }
                };

                return Task.CompletedTask;
            });

            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
            options.AddOperationTransformer<BearerSecurityOperationTransformer>();
        });

        return services;
    }

    public static WebApplication MapOpenApiUi(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();

            app.MapScalarApiReference(options =>
            {
                options.Title = "DroneBuilder API";
                options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
            });
        }

        return app;
    }
}
