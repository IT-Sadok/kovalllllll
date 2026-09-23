using DroneBuilder.Application.Common.Errors;
using DroneBuilder.Application.Common.Pagination;

namespace DroneBuilder.API.Common.Responses;

public static class ApiResults
{
    public static IResult Ok() => Results.Ok(new ApiResponse { Success = true });

    public static IResult Ok<T>(T data) => Results.Ok(new ApiResponse<T> { Success = true, Data = data });

    public static IResult Paged<T>(PagedResult<T> page) => Results.Ok(new ApiResponse<IEnumerable<T>>
    {
        Success = true,
        Data = page.Items,
        Pagination = new ApiPagination
        {
            PageNumber = page.Page,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount
        }
    });

    public static IResult Error(int statusCode, string code, string message)
        => Results.Json(Failure(code, message), statusCode: statusCode);

    public static IResult Error(int statusCode, IReadOnlyList<ApiError> errors)
        => Results.Json(new ApiResponse { Success = false, Errors = errors }, statusCode: statusCode);

    public static ApiResponse Failure(string code, string message) => new()
    {
        Success = false,
        Errors = [new ApiError { Code = code, Message = message }]
    };

    public static string CodeFor(int statusCode) => statusCode switch
    {
        StatusCodes.Status401Unauthorized => AppError.Codes.Unauthorized,
        StatusCodes.Status403Forbidden => AppError.Codes.Forbidden,
        StatusCodes.Status404NotFound => AppError.Codes.NotFound,
        StatusCodes.Status409Conflict => AppError.Codes.Conflict,
        StatusCodes.Status429TooManyRequests => AppError.Codes.TooManyRequests,
        >= StatusCodes.Status500InternalServerError => AppError.Codes.Internal,
        _ => AppError.Codes.BadRequest
    };
}
