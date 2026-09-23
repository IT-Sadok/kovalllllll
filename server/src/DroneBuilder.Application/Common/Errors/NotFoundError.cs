namespace DroneBuilder.Application.Common.Errors;

public class NotFoundError : AppError
{
    public NotFoundError(string message) : base(message, Codes.NotFound) { }
}
