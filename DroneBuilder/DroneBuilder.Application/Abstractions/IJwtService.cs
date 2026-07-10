namespace DroneBuilder.Application.Abstractions;

public interface IJwtService
{
    Task<string> GenerateJwtTokenAsync(string userId);
}
