using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace DroneBuilder.API.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var (statusCode, title, errors) = MapException(exception);

            logger.LogError(exception, 
                "Exception occurred: {Message}. StatusCode: {StatusCode}", 
                exception.Message, 
                statusCode);

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = exception.Message,
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

        private static (int StatusCode, string Title, object? Errors) MapException(Exception exception)
        {
            return exception switch
            {
                Application.Exceptions.NotFoundException => 
                    (404, "Not Found", null),
                
                Application.Exceptions.ValidationException validationEx => 
                    (400, "Validation Error", validationEx.Errors),
                
                Application.Exceptions.BadRequestException => 
                    (400, "Bad Request", null),
                
                Application.Exceptions.InvalidEmailOrPasswordException => 
                    (401, "Invalid Credentials", null),
                
                Application.Exceptions.UnauthorizedException => 
                    (401, "Unauthorized", null),
                
                Application.Exceptions.ForbiddenException => 
                    (403, "Forbidden", null),

                ArgumentNullException or ArgumentException => 
                    (400, "Bad Request", null),
                
                _ => (500, "Internal Server Error", null)
            };
        }
    }
