using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Services;

public interface IJwtService
{
    Task<string> GenerateJwtTokenAsync(User user);
}