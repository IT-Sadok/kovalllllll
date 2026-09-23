using FluentResults;

namespace DroneBuilder.Application.Common.ResultErrors;

public class BadRequestError : Error
{
    public BadRequestError(string message) : base(message) { }
}
