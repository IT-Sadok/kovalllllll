using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DroneBuilder.Infrastructure.Repositories;

public class BuildRepository(ApplicationDbContext dbContext) : IBuildRepository
{
    public async Task AddAsync(Build build, CancellationToken cancellationToken = default)
    {
        await dbContext.Builds.AddAsync(build, cancellationToken);
    }

    public async Task<Build?> GetUserBuildAsync(Guid buildId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Builds
            .Include(b => b.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p!.Images)
            .AsSplitQuery()
            .FirstOrDefaultAsync(b => b.Id == buildId && b.UserId == userId, cancellationToken);
    }

    public async Task<ICollection<Build>> GetUserBuildsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Builds
            .AsNoTracking()
            .Include(b => b.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p!.Images)
            .AsSplitQuery()
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public void Remove(Build build)
    {
        dbContext.Builds.Remove(build);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
