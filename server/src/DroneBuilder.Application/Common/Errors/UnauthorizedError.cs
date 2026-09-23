using FluentResults;

namespace DroneBuilder.Application.Common.Errors;

public class UnauthorizedError : Error
{
    public UnauthorizedError(string message) : base(message) { }
}
