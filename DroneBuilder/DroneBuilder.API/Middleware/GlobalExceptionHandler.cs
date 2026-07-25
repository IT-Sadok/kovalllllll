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
        (int statusCode, string title) = MapException(exception);

        logger.LogError(
            exception,
            "Exception occurred: {Message}. StatusCode: {StatusCode}",
            exception.Message,
            statusCode);

        string detail = exception switch
        {
            DbUpdateConcurrencyException => "The resource was changed by another operation. Retry the request.",
            DbUpdateException => "The requested change conflicts with the current data state.",
            _ when statusCode == StatusCodes.Status500InternalServerError => "An unexpected error occurred.",
            _ => exception.Message
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };
        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static (int StatusCode, string Title) MapException(Exception exception)
    {
        return exception switch
        {
            UnauthorizedAccessException =>
                (StatusCodes.Status401Unauthorized, "Unauthorized"),

            DbUpdateConcurrencyException or DbUpdateException =>
                (StatusCodes.Status409Conflict, "Conflict"),

            ArgumentNullException or ArgumentException =>
                (StatusCodes.Status400BadRequest, "Bad Request"),

            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };
    }
}
