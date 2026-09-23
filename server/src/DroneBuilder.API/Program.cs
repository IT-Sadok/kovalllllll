using DroneBuilder.API.Common.Extensions;
using DroneBuilder.API.Common.Middleware;
using DroneBuilder.Application;
using DroneBuilder.Infrastructure;
using Microsoft.AspNetCore.HttpOverrides;

namespace DroneBuilder.API;

public abstract class Program
{
    public static async Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOpenApiConfig();

        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        builder.Services.AddApplication()
            .AddInfrastructure(builder.Configuration)
            .AddAuth(builder.Configuration);

        builder.Services.AddRateLimitingConfig(builder.Configuration);

        bool trustForwardedHeaders = builder.Configuration.GetValue<bool>("ForwardedHeaders:Enabled");

        if (trustForwardedHeaders)
        {
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });
        }

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
        app.UseApiStatusCodePages();

        if (trustForwardedHeaders)
        {
            app.UseForwardedHeaders();
        }

        app.UseHttpsRedirection();

        app.MapOpenApiUi();

        app.UseCors("ConfiguredOrigins");

        app.UseRateLimiter();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseSpaStaticFiles();
        app.MapApplicationEndpoints();
        app.MapSpaFallback();

        await app.RunAsync();
    }
}
