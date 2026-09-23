using FluentResults;

namespace DroneBuilder.Application.Common.ResultErrors;

public class UnauthorizedError : Error
{
    public UnauthorizedError(string message) : base(message) { }
}
