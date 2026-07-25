using DroneBuilder.Application.Features.Catalog.ProductVariants.Models;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.ProductVariants.GetVariantSpecifications;

public sealed class GetVariantSpecificationsQueryHandler(ICatalogSpecificationRepository repository)
    : IQueryHandler<GetVariantSpecificationsQuery, ICollection<ProductVariantSpecificationModel>>
{
    public async Task<Result<ICollection<ProductVariantSpecificationModel>>> ExecuteAsync(
        GetVariantSpecificationsQuery query,
        CancellationToken cancellationToken)
    {
        Product? product = await repository.GetProductAsync(query.ProductId, cancellationToken);
        if (product is null || product.PublicationStatus != ProductPublicationStatus.Published)
        {
            return Result.Fail<ICollection<ProductVariantSpecificationModel>>(new NotFoundError(
                $"Product with id {query.ProductId} not found."));
        }

        ProductVariant? variant = product.Variants.FirstOrDefault(
            item => item.Id == query.VariantId && item.IsActive);
        if (variant is null)
        {
            return Result.Fail<ICollection<ProductVariantSpecificationModel>>(new NotFoundError(
                $"Variant with id {query.VariantId} not found."));
        }

        return Result.Ok<ICollection<ProductVariantSpecificationModel>>(variant.Specifications
            .OrderBy(specification => specification.Property!.Name)
            .Select(specification => specification.ToModel())
            .ToList());
    }
}

public sealed record GetVariantSpecificationsQuery(Guid ProductId, Guid VariantId);
