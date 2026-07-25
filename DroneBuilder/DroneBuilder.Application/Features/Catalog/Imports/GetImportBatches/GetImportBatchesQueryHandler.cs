using DroneBuilder.Application.Features.Catalog.Imports.Models;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Imports.GetImportBatches;

public sealed class GetImportBatchesQueryHandler(ICatalogImportRepository repository)
    : IQueryHandler<GetImportBatchesQuery, ICollection<ImportBatchModel>>
{
    public async Task<Result<ICollection<ImportBatchModel>>> ExecuteAsync(
        GetImportBatchesQuery query,
        CancellationToken cancellationToken)
    {
        ImportSource? source = await repository.GetSourceAsync(query.SourceId, cancellationToken);
        if (source is null)
        {
            return Result.Fail<ICollection<ImportBatchModel>>(new NotFoundError(
                $"Import source with id {query.SourceId} not found."));
        }

        return Result.Ok<ICollection<ImportBatchModel>>(
            (await repository.GetBatchesAsync(query.SourceId, cancellationToken))
                .Select(batch => batch.ToModel())
                .ToList());
    }
}

public sealed record GetImportBatchesQuery(Guid SourceId);
