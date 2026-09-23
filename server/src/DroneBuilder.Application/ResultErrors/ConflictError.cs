using FluentResults;

namespace DroneBuilder.Application.ResultErrors;

public class ConflictError : Error
{
    public ConflictError(string message) : base(message) { }
}
