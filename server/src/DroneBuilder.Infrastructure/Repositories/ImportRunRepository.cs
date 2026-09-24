using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DroneBuilder.Infrastructure.Repositories;

public class ImportRunRepository(ApplicationDbContext dbContext) : IImportRunRepository
{
    public async Task AddAsync(ImportRun run, CancellationToken cancellationToken = default)
    {
        await dbContext.ImportRuns.AddAsync(run, cancellationToken);
    }

    public async Task<ImportRun?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.ImportRuns.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<ICollection<ImportRun>> GetRecentAsync(int count, CancellationToken cancellationToken = default)
    {
        return await dbContext.ImportRuns
            .AsNoTracking()
            .OrderByDescending(r => r.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasActiveRunAsync(string source, CancellationToken cancellationToken = default)
    {
        return await dbContext.ImportRuns.AnyAsync(
            r => r.Source == source && (r.Status == ImportRunStatus.Queued || r.Status == ImportRunStatus.Running),
            cancellationToken);
    }

    public async Task<int> FailUnfinishedRunsAsync(string error, DateTime finishedAt,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.ImportRuns
            .Where(r => r.Status == ImportRunStatus.Queued || r.Status == ImportRunStatus.Running)
            .ExecuteUpdateAsync(s => s
                .SetProperty(r => r.Status, ImportRunStatus.Failed)
                .SetProperty(r => r.Error, error)
                .SetProperty(r => r.FinishedAt, finishedAt), cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
