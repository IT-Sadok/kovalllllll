using FluentResults;

namespace DroneBuilder.Application.Common.Errors;

public class BadRequestError : Error
{
    public BadRequestError(string message) : base(message) { }
}
