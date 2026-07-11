using FluentResults;

namespace DroneBuilder.Application.ResultErrors;

public class NotFoundError : Error
{
    public NotFoundError(string message) : base(message) { }
}
