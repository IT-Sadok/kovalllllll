using FluentResults;

namespace DroneBuilder.Application.ResultErrors;

public class UnauthorizedError : Error
{
    public UnauthorizedError(string message) : base(message) { }
}
