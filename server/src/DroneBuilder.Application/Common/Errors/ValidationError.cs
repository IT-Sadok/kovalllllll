namespace DroneBuilder.Application.Common.Errors;

public class ValidationError : AppError
{
    public IDictionary<string, string[]>? ErrorDetails { get; }

    public ValidationError(string message) : base(message, Codes.Validation) { }

    public ValidationError(string message, IDictionary<string, string[]> errorDetails) : base(message, Codes.Validation)
    {
        ErrorDetails = errorDetails;
    }
}
