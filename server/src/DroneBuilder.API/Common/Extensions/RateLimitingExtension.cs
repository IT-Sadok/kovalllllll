using System.Threading.RateLimiting;
using DroneBuilder.API.Common.Options;
using DroneBuilder.API.Common.Responses;
using DroneBuilder.Application.Common.Errors;
using DroneBuilder.Application.Common.Validation.Options;
using FluentValidation;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace DroneBuilder.API.Common.Extensions;

public static class RateLimitingExtension
{
    public const string LoginPolicy = "LoginPolicy";
    public const string EmailPolicy = "EmailPolicy";

    public static IServiceCollection AddRateLimitingConfig(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatorsFromAssembly(typeof(RateLimitingExtension).Assembly);

        services.AddOptions<RateLimitPolicyOptions>(LoginPolicy)
            .Bind(configuration.GetSection("RateLimiting:Login"))
            .ValidateFluentValidation()
            .ValidateOnStart();

        services.AddOptions<RateLimitPolicyOptions>(EmailPolicy)
            .Bind(configuration.GetSection("RateLimiting:Email"))
            .ValidateFluentValidation()
            .ValidateOnStart();

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

                await context.HttpContext.Response.WriteAsJsonAsync(
                    ApiResults.Failure(AppError.Codes.TooManyRequests, "Too many requests. Please try again later."),
                    cancellationToken: token);
            };
        });

        services.AddOptions<RateLimiterOptions>()
            .Configure<IOptionsMonitor<RateLimitPolicyOptions>>((options, policyOptions) =>
            {
                options.AddPolicy(LoginPolicy, context => CreatePerIpLimiter(context, policyOptions.Get(LoginPolicy)));
                options.AddPolicy(EmailPolicy, context => CreatePerIpLimiter(context, policyOptions.Get(EmailPolicy)));
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
