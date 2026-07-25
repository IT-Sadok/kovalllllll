using DroneBuilder.Application.Features.Catalog.Imports.Models;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Imports.GetVariantExternalReferences;

public sealed class GetVariantExternalReferencesQueryHandler(ICatalogImportRepository repository)
    : IQueryHandler<GetVariantExternalReferencesQuery, ICollection<ExternalReferenceModel>>
{
    public async Task<Result<ICollection<ExternalReferenceModel>>> ExecuteAsync(
        GetVariantExternalReferencesQuery query,
        CancellationToken cancellationToken)
    {
        Product? product = await repository.GetProductAsync(query.ProductId, cancellationToken);
        ProductVariant? variant = product?.Variants.FirstOrDefault(item => item.Id == query.VariantId);
        return variant is null
            ? Result.Fail<ICollection<ExternalReferenceModel>>(new NotFoundError(
                $"Variant with id {query.VariantId} not found."))
            : Result.Ok<ICollection<ExternalReferenceModel>>(
                variant.ExternalReferences.Select(reference => reference.ToModel()).ToList());
    }
}

public sealed record GetVariantExternalReferencesQuery(Guid ProductId, Guid VariantId);
