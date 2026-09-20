using DroneBuilder.API.Extensions;
using DroneBuilder.API.Middleware;
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

        // Off by default on purpose. X-Forwarded-For is client supplied, so honouring it without a
        // reverse proxy in front lets anyone forge their address and walk past the login rate limit.
        // Turn it on only where every request really does arrive through a trusted proxy.
        bool trustForwardedHeaders = builder.Configuration.GetValue<bool>("ForwardedHeaders:Enabled");

        if (trustForwardedHeaders)
        {
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

                // The defaults trust loopback only, which never matches a platform proxy such as
                // Azure Web Apps; the app is not reachable except through it.
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

        // Must run before anything reads the scheme or the client address: HTTPS redirection would
        // otherwise loop behind a TLS terminating proxy, and the rate limiter would partition every
        // request under the proxy's own address.
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
