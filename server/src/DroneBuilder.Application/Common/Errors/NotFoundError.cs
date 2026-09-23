using FluentResults;

namespace DroneBuilder.Application.Common.Errors;

public class NotFoundError : Error
{
    public NotFoundError(string message) : base(message) { }
}
