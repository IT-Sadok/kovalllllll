using DroneBuilder.Application.Features.Catalog.Imports.Models;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Imports.GetImportItems;

public sealed class GetImportItemsQueryHandler(ICatalogImportRepository repository)
    : IQueryHandler<GetImportItemsQuery, ICollection<ImportItemModel>>
{
    public async Task<Result<ICollection<ImportItemModel>>> ExecuteAsync(
        GetImportItemsQuery query,
        CancellationToken cancellationToken)
    {
        ImportBatch? batch = await repository.GetBatchAsync(query.SourceId, query.BatchId, cancellationToken);
        if (batch is null)
        {
            return Result.Fail<ICollection<ImportItemModel>>(new NotFoundError(
                $"Import batch with id {query.BatchId} not found."));
        }

        return Result.Ok<ICollection<ImportItemModel>>(batch.Items
            .OrderBy(item => item.CreatedAt)
            .Select(item => item.ToModel())
            .ToList());
    }
}

public sealed record GetImportItemsQuery(Guid SourceId, Guid BatchId);
