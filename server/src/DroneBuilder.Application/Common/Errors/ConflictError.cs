using FluentResults;

namespace DroneBuilder.Application.Common.Errors;

public class ConflictError : Error
{
    public ConflictError(string message) : base(message) { }
}
