using FluentResults;

namespace DroneBuilder.Application.Common.Abstractions;

public interface IJwtService
{
    Task<Result<string>> GenerateJwtTokenAsync(string userId);
}
