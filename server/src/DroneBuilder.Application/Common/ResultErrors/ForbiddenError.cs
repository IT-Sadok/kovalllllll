using FluentResults;

namespace DroneBuilder.Application.Common.ResultErrors;

public class ForbiddenError : Error
{
    public ForbiddenError(string message) : base(message) { }
}
