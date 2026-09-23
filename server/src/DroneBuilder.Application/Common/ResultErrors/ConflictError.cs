using FluentResults;

namespace DroneBuilder.Application.Common.ResultErrors;

public class ConflictError : Error
{
    public ConflictError(string message) : base(message) { }
}
