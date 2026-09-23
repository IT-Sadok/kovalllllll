using System.Threading.RateLimiting;
using DroneBuilder.API.Options;

namespace DroneBuilder.API.Extensions;

public static class RateLimitingExtension
{
    public const string LoginPolicy = "LoginPolicy";
    public const string EmailPolicy = "EmailPolicy";

    public static IServiceCollection AddRateLimitingConfig(this IServiceCollection services, IConfiguration configuration)
    {
        RateLimitPolicyOptions loginOptions = configuration.GetSection("RateLimiting:Login").Get<RateLimitPolicyOptions>()
                                              ?? new RateLimitPolicyOptions();

        RateLimitPolicyOptions emailOptions = configuration.GetSection("RateLimiting:Email").Get<RateLimitPolicyOptions>()
                                              ?? new RateLimitPolicyOptions { PermitLimit = 5 };

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, token) =>
            {
                ILogger<Program> logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                string ipAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown IP";

                logger.LogWarning("Rate limit exceeded for IP {IpAddress} on {Path}",
                    ipAddress, context.HttpContext.Request.Path);

                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
                }

                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    Error = "Too many requests. Please try again later.",
                    RetryAfterSeconds = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan wait) ? (int)wait.TotalSeconds : 0
                }, cancellationToken: token);
            };

            options.AddPolicy(LoginPolicy, context => CreatePerIpLimiter(context, loginOptions));
            options.AddPolicy(EmailPolicy, context => CreatePerIpLimiter(context, emailOptions));
        });

        return services;
    }

    private static RateLimitPartition<string> CreatePerIpLimiter(HttpContext context, RateLimitPolicyOptions policyOptions)
    {
        string ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ipAddress,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = policyOptions.PermitLimit,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(policyOptions.WindowInMinutes)
            });
    }
}
