using DroneBuilder.API.Contexts;
using DroneBuilder.API.Extensions;
using DroneBuilder.API.Middleware;
using DroneBuilder.Application;
using DroneBuilder.Application.Contexts;
using DroneBuilder.Infrastructure;

namespace DroneBuilder.API;

public abstract class Program
{
    public static async Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IUserContext, UserContext>();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOpenApiConfig();

        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        builder.Services.AddApplication()
            .AddInfrastructure(builder.Configuration)
            .AddAuth(builder.Configuration);

        builder.Services.AddRateLimitingConfig(builder.Configuration);

        string[] allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>()?
            .Where(origin => !string.IsNullOrWhiteSpace(origin))
            .Select(origin => origin.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray() ?? [];

        if (allowedOrigins.Contains("*", StringComparer.Ordinal))
        {
            throw new InvalidOperationException("Cors:AllowedOrigins must contain explicit origins; wildcard is not allowed.");
        }

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("ConfiguredOrigins", policy =>
            {
                policy.AllowAnyMethod()
                    .AllowAnyHeader();

                if (allowedOrigins.Length > 0)
                {
                    policy.WithOrigins(allowedOrigins);
                }
            });
        });

        WebApplication app = builder.Build();

        await app.InitializeDatabaseAsync();

        app.UseExceptionHandler();
        app.MapOpenApiUi();

        app.UseCors("ConfiguredOrigins");

        app.UseRateLimiter();

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseHttpsRedirection();

        app.UseSpaStaticFiles();
        app.MapApplicationEndpoints();
        app.MapSpaFallback();

        await app.RunAsync();
    }
}
