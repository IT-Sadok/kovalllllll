using Microsoft.OpenApi.Models;

namespace DroneBuilder.API.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerGen(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "DroneBuilder API",
                Version = "v1",
                Description = "API for managing drone products and configurations."
            });

            c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Введіть свій JWT токен у це поле. Приклад: eyJhbGciOiJIUz..."
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    []
                }
            });
        });


        return services;
    }
}