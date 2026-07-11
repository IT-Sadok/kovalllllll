using FluentResults;

namespace DroneBuilder.Application.Abstractions;

public interface IJwtService
{
    Task<Result<string>> GenerateJwtTokenAsync(string userId);
}
