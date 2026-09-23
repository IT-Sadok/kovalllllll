using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Common.Repositories;

public interface IUserRepository
{
    Task AddUserAsync(User user, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
