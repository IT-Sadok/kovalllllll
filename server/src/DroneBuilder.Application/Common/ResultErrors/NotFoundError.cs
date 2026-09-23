using FluentResults;

namespace DroneBuilder.Application.Common.ResultErrors;

public class NotFoundError : Error
{
    public NotFoundError(string message) : base(message) { }
}
