using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Infrastructure.Repositories;

public class UserRepository(ApplicationDbContext dbContext) : IUserRepository
{
    public async Task AddUserAsync(User user, CancellationToken cancellationToken = default)
    {
        await dbContext.Users.AddAsync(user, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}