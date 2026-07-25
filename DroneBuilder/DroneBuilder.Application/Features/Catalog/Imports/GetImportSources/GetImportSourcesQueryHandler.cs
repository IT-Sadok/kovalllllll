using DroneBuilder.Application.Features.Catalog.Imports.Models;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Imports.GetImportSources;

public sealed class GetImportSourcesQueryHandler(ICatalogImportRepository repository)
    : IQueryHandler<GetImportSourcesQuery, ICollection<ImportSourceModel>>
{
    public async Task<Result<ICollection<ImportSourceModel>>> ExecuteAsync(
        GetImportSourcesQuery query,
        CancellationToken cancellationToken)
    {
        return Result.Ok<ICollection<ImportSourceModel>>(
            (await repository.GetSourcesAsync(cancellationToken)).Select(source => source.ToModel()).ToList());
    }
}

public sealed record GetImportSourcesQuery;
