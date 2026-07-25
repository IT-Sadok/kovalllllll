using DroneBuilder.Application.ResultErrors;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using ValidationError = DroneBuilder.Application.ResultErrors.ValidationError;

namespace DroneBuilder.API.Extensions;

public static class ResultExtensions
{
    public static IResult ToHttpResult(this Result result)
    {
        if (result.IsSuccess)
        {
            return Results.NoContent();
        }

        return MapError(result);
    }

    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        return MapError(result);
    }

    private static IResult MapError(ResultBase result)
    {
        IError error = result.Errors.First();

        var problemDetails = new ProblemDetails
        {
            Detail = error.Message
        };

        return error switch
        {
            NotFoundError => Results.Problem(statusCode: 404, title: "Not Found", detail: error.Message),
            ValidationError validationError => CreateValidationProblem(validationError),
            BadRequestError => Results.Problem(statusCode: 400, title: "Bad Request", detail: error.Message),
            UnauthorizedError => Results.Problem(statusCode: 401, title: "Unauthorized", detail: error.Message),
            ForbiddenError => Results.Problem(statusCode: 403, title: "Forbidden", detail: error.Message),
            ConflictError => Results.Problem(statusCode: 409, title: "Conflict", detail: error.Message),
            _ => Results.Problem(statusCode: 500, title: "Internal Server Error", detail: "An unexpected error occurred."),
        };
    }

    private static IResult CreateValidationProblem(ValidationError validationError)
    {
        if (validationError.ErrorDetails != null)
        {
            return Results.ValidationProblem(
                validationError.ErrorDetails,
                detail: validationError.Message,
                title: "Validation Error",
                statusCode: 400);
        }

        return Results.Problem(statusCode: 400, title: "Validation Error", detail: validationError.Message);
    }
}
