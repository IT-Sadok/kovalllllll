using DroneBuilder.API.Common.Responses;
using DroneBuilder.Application.Common.Errors;
using DroneBuilder.Application.Common.Pagination;
using FluentResults;

namespace DroneBuilder.API.Common.Extensions;

public static class ResultExtensions
{
    public static IResult ToHttpResult(this Result result)
    {
        if (result.IsSuccess)
        {
            return ApiResults.Ok();
        }

        return MapError(result);
    }

    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return ApiResults.Ok(result.Value);
        }

        return MapError(result);
    }

    public static IResult ToHttpResult<T>(this Result<PagedResult<T>> result)
    {
        if (result.IsSuccess)
        {
            return ApiResults.Paged(result.Value);
        }

        return MapError(result);
    }

    private static IResult MapError(ResultBase result)
    {
        IError error = result.Errors.First();

        return error switch
        {
            ValidationError { ErrorDetails.Count: > 0 } validationError => CreateValidationResult(validationError),
            AppError appError => ApiResults.Error(StatusCodeFor(appError), appError.Code, appError.Message),
            _ => ApiResults.Error(StatusCodes.Status500InternalServerError, AppError.Codes.Internal, error.Message),
        };
    }

    private static int StatusCodeFor(AppError error) => error switch
    {
        NotFoundError => StatusCodes.Status404NotFound,
        ValidationError or BadRequestError => StatusCodes.Status400BadRequest,
        UnauthorizedError => StatusCodes.Status401Unauthorized,
        ForbiddenError => StatusCodes.Status403Forbidden,
        ConflictError => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError,
    };

    private static IResult CreateValidationResult(ValidationError validationError)
    {
        ApiError[] errors = validationError.ErrorDetails!
            .SelectMany(field => field.Value.Select(message => new ApiError
            {
                Code = validationError.Code,
                Field = field.Key,
                Message = message
            }))
            .ToArray();

        return ApiResults.Error(StatusCodes.Status400BadRequest, errors);
    }
}
