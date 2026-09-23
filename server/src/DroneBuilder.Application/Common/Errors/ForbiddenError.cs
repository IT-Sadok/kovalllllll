namespace DroneBuilder.Application.Common.Errors;

public class ForbiddenError : AppError
{
    public ForbiddenError(string message) : base(message, Codes.Forbidden) { }
}
