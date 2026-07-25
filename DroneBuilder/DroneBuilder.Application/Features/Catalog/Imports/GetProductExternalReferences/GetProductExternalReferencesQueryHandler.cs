using DroneBuilder.Application.Features.Catalog.Imports.Models;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Imports.GetProductExternalReferences;

public sealed class GetProductExternalReferencesQueryHandler(ICatalogImportRepository repository)
    : IQueryHandler<GetProductExternalReferencesQuery, ICollection<ExternalReferenceModel>>
{
    public async Task<Result<ICollection<ExternalReferenceModel>>> ExecuteAsync(
        GetProductExternalReferencesQuery query,
        CancellationToken cancellationToken)
    {
        Product? product = await repository.GetProductAsync(query.ProductId, cancellationToken);
        return product is null
            ? Result.Fail<ICollection<ExternalReferenceModel>>(new NotFoundError(
                $"Product with id {query.ProductId} not found."))
            : Result.Ok<ICollection<ExternalReferenceModel>>(
                product.ExternalReferences.Select(reference => reference.ToModel()).ToList());
    }
}

public sealed record GetProductExternalReferencesQuery(Guid ProductId);
