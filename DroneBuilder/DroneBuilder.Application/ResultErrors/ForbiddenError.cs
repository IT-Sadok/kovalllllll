using FluentResults;

namespace DroneBuilder.Application.ResultErrors;

public class ForbiddenError : Error
{
    public ForbiddenError(string message) : base(message) { }
}
