using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DroneBuilder.API.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
    {
        (int statusCode, string? title, string detail) = MapException(exception);

        logger.LogError(exception,
            "Exception occurred: {Message}. StatusCode: {StatusCode}",
            exception.Message,
            statusCode);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static (int StatusCode, string Title, string Detail) MapException(Exception exception)
    {
        return exception switch
        {
            UnauthorizedAccessException =>
                (401, "Unauthorized", "Authentication is required."),

            DbUpdateConcurrencyException =>
                (409, "Conflict", "The record was changed by another request. Please try again."),

            ArgumentNullException or ArgumentException =>
                (400, "Bad Request", exception.Message),

            _ => (500, "Internal Server Error", "An unexpected error occurred.")
        };
    }
}
