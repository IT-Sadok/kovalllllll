using FluentResults;

namespace DroneBuilder.Application.Common.Errors;

public abstract class AppError : Error
{
    protected AppError(string message, string code) : base(message)
    {
        Code = code;
    }

    public string Code { get; }

    public static class Codes
    {
        public const string BadRequest = "BAD_REQUEST";
        public const string Validation = "VALIDATION_ERROR";
        public const string Unauthorized = "UNAUTHORIZED";
        public const string Forbidden = "FORBIDDEN";
        public const string NotFound = "NOT_FOUND";
        public const string Conflict = "CONFLICT";
        public const string TooManyRequests = "TOO_MANY_REQUESTS";
        public const string Internal = "INTERNAL_ERROR";
    }
}
