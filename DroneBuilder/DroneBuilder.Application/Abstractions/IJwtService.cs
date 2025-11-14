using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Abstractions;

public interface IJwtService
{
    Task<string> GenerateJwtTokenAsync(User user);
}