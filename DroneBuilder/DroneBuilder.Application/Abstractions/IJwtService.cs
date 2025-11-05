using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application;

public interface IJwtService
{
    Task<string> GenerateJwtTokenAsync(User user);
}