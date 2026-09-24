using DroneBuilder.Application.Common.Mediator.Interfaces;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Imports.GetImportRuns;

public class GetImportRunsQueryHandler(IImportRunRepository importRunRepository)
    : IQueryHandler<GetImportRunsQuery, ICollection<ImportRunModel>>
{
    private const int RecentRuns = 20;

    public async Task<Result<ICollection<ImportRunModel>>> ExecuteAsync(GetImportRunsQuery query,
        CancellationToken cancellationToken)
    {
        ICollection<ImportRun> runs = await importRunRepository.GetRecentAsync(RecentRuns, cancellationToken);
        return Result.Ok<ICollection<ImportRunModel>>(runs.Select(r => r.ToModel()).ToList());
    }
}

public record GetImportRunsQuery;
