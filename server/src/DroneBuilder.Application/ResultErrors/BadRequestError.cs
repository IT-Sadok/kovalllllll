using FluentResults;

namespace DroneBuilder.Application.ResultErrors;

public class BadRequestError : Error
{
    public BadRequestError(string message) : base(message) { }
}
