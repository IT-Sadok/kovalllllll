using FluentResults;

namespace DroneBuilder.Application.Common.Errors;

public class ForbiddenError : Error
{
    public ForbiddenError(string message) : base(message) { }
}
