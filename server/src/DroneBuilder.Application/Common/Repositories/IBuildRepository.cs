using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Common.Repositories;

public interface IBuildRepository
{
    Task AddAsync(Build build, CancellationToken cancellationToken = default);
    Task<Build?> GetUserBuildAsync(Guid buildId, Guid userId, CancellationToken cancellationToken = default);
    Task<ICollection<Build>> GetUserBuildsAsync(Guid userId, CancellationToken cancellationToken = default);
    void Remove(Build build);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
