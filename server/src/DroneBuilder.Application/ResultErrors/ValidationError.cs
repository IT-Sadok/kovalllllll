using FluentResults;

namespace DroneBuilder.Application.ResultErrors;

public class ValidationError : Error
{
    public IDictionary<string, string[]>? ErrorDetails { get; }

    public ValidationError(string message) : base(message) { }

    public ValidationError(string message, IDictionary<string, string[]> errorDetails) : base(message)
    {
        ErrorDetails = errorDetails;
    }
}
