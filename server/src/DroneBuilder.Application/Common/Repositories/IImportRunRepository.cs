using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Common.Repositories;

public interface IImportRunRepository
{
    Task AddAsync(ImportRun run, CancellationToken cancellationToken = default);
    Task<ImportRun?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ICollection<ImportRun>> GetRecentAsync(int count, CancellationToken cancellationToken = default);
    Task<bool> HasActiveRunAsync(string source, CancellationToken cancellationToken = default);
    Task<int> FailUnfinishedRunsAsync(string error, DateTime finishedAt, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
