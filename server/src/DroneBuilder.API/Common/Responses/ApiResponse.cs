namespace DroneBuilder.API.Common.Responses;

public class ApiResponse
{
    public required bool Success { get; init; }
    public IReadOnlyList<ApiError> Errors { get; init; } = [];
}

public sealed class ApiResponse<T> : ApiResponse
{
    public T? Data { get; init; }
    public ApiPagination? Pagination { get; init; }
}

public sealed class ApiPagination
{
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
}

public sealed class ApiError
{
    public required string Code { get; init; }
    public string? Field { get; init; }
    public required string Message { get; init; }
}
