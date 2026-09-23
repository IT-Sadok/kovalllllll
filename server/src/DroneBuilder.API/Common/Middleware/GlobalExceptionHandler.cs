using DroneBuilder.API.Common.Responses;
using DroneBuilder.Application.Common.Errors;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace DroneBuilder.API.Common.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
    {
        (int statusCode, string code, string message) = MapException(exception);

        logger.LogError(exception,
            "Exception occurred: {Message}. StatusCode: {StatusCode}",
            exception.Message,
            statusCode);

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(ApiResults.Failure(code, message), cancellationToken);

        return true;
    }

    private static (int StatusCode, string Code, string Message) MapException(Exception exception)
    {
        return exception switch
        {
            UnauthorizedAccessException =>
                (401, AppError.Codes.Unauthorized, "Authentication is required."),

            DbUpdateConcurrencyException =>
                (409, AppError.Codes.Conflict, "The record was changed by another request. Please try again."),

            BadHttpRequestException badRequest =>
                (badRequest.StatusCode, ApiResults.CodeFor(badRequest.StatusCode), exception.Message),

            ArgumentNullException or ArgumentException =>
                (400, AppError.Codes.BadRequest, exception.Message),

            _ => (500, AppError.Codes.Internal, "An unexpected error occurred.")
        };
    }
}
