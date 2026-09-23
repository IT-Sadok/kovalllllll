namespace DroneBuilder.Application.Common.Errors;

public class BadRequestError : AppError
{
    public BadRequestError(string message) : base(message, Codes.BadRequest) { }
}
