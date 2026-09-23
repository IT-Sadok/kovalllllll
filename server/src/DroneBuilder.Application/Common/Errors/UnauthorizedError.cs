namespace DroneBuilder.Application.Common.Errors;

public class UnauthorizedError : AppError
{
    public UnauthorizedError(string message) : base(message, Codes.Unauthorized) { }
}
