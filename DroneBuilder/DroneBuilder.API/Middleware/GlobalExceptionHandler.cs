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
        (int statusCode, string? title, string detail, object? errors) = MapException(exception);

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

        if (errors != null && exception is Application.Exceptions.ValidationException)
        {
            problemDetails.Extensions["errors"] = errors;
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static (int StatusCode, string Title, string Detail, object? Errors) MapException(Exception exception)
    {
        return exception switch
        {
            Application.Exceptions.NotFoundException =>
                (404, "Not Found", exception.Message, null),

            Application.Exceptions.ValidationException validationEx =>
                (400, "Validation Error", exception.Message, validationEx.Errors),

            Application.Exceptions.BadRequestException =>
                (400, "Bad Request", exception.Message, null),

            Application.Exceptions.InvalidEmailOrPasswordException =>
                (401, "Invalid Credentials", exception.Message, null),

            Application.Exceptions.UnauthorizedException =>
                (401, "Unauthorized", exception.Message, null),

            Application.Exceptions.ForbiddenException =>
                (403, "Forbidden", exception.Message, null),

            // Thrown by UserContext when the NameIdentifier claim is missing.
            UnauthorizedAccessException =>
                (401, "Unauthorized", "Authentication is required.", null),

            // Raised when a concurrency token no longer matches, i.e. someone else won the race.
            DbUpdateConcurrencyException =>
                (409, "Conflict", "The record was changed by another request. Please try again.", null),

            ArgumentNullException or ArgumentException =>
                (400, "Bad Request", exception.Message, null),

            // The message of an unexpected exception can carry connection strings, SQL and file paths,
            // so it must never reach the client.
            _ => (500, "Internal Server Error", "An unexpected error occurred.", null)
        };
    }
}
