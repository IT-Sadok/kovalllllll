namespace DroneBuilder.Application.Common.Errors;

public class ConflictError : AppError
{
    public ConflictError(string message) : base(message, Codes.Conflict) { }
}
