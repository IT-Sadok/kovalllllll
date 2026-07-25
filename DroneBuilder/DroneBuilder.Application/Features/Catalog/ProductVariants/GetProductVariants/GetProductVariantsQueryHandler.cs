using DroneBuilder.Application.Features.Catalog.ProductVariants.Models;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.ProductVariants.GetProductVariants;

public sealed class GetProductVariantsQueryHandler(ICatalogSpecificationRepository repository)
    : IQueryHandler<GetProductVariantsQuery, ICollection<ProductVariantModel>>
{
    public async Task<Result<ICollection<ProductVariantModel>>> ExecuteAsync(
        GetProductVariantsQuery query,
        CancellationToken cancellationToken)
    {
        Product? product = await repository.GetProductAsync(query.ProductId, cancellationToken);
        if (product is null || product.PublicationStatus != ProductPublicationStatus.Published)
        {
            return Result.Fail<ICollection<ProductVariantModel>>(new NotFoundError(
                $"Product with id {query.ProductId} not found."));
        }

        return Result.Ok<ICollection<ProductVariantModel>>(product.Variants
            .Where(variant => variant.IsActive)
            .OrderByDescending(variant => variant.IsDefault)
            .ThenBy(variant => variant.Name)
            .ThenBy(variant => variant.Sku)
            .Select(variant => variant.ToModel())
            .ToList());
    }
}

public sealed record GetProductVariantsQuery(Guid ProductId);
