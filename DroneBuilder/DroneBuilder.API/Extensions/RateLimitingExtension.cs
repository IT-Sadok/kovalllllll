using System.Threading.RateLimiting;
using DroneBuilder.API.Options;

namespace DroneBuilder.API.Extensions;

public static class RateLimitingExtension
{
    public static IServiceCollection AddRateLimitingConfig(this IServiceCollection services, IConfiguration configuration)
    {
        var rateLimitConfig = configuration.GetSection("RateLimiting:Login").Get<LoginRateLimitOptions>() ?? new LoginRateLimitOptions();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, token) =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                var ipAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown IP";
                
                logger.LogWarning("Login rate limit exceeded for IP {IpAddress}", ipAddress);

                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
                }

                await context.HttpContext.Response.WriteAsJsonAsync(new 
                {
                    Error = "Too many requests. Please try again later.",
                    RetryAfterSeconds = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var wait) ? (int)wait.TotalSeconds : 0
                }, cancellationToken: token);
            };

            options.AddPolicy("LoginPolicy", context =>
            {
                var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ipAddress,
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = rateLimitConfig.PermitLimit,
                        QueueLimit = 0,
                        Window = TimeSpan.FromMinutes(rateLimitConfig.WindowInMinutes)
                    });
            });
        });

        return services;
    }
}
