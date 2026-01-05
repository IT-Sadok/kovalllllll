namespace DroneBuilder.Application.Exceptions;

public class ValidationException : Exception
{
    public IDictionary<string, string[]>? Errors { get; }

    public ValidationException() : base()
    {
    }

    public ValidationException(string message) : base(message)
    {
    }

    public ValidationException(string message, IDictionary<string, string[]> errors) : base(message)
    {
        Errors = errors;
    }

    public ValidationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}