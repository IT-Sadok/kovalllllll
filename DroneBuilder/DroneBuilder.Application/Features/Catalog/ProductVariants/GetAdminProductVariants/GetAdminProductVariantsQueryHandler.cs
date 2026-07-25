using DroneBuilder.Application.Features.Catalog.ProductVariants.Models;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.ProductVariants.GetAdminProductVariants;

public sealed class GetAdminProductVariantsQueryHandler(ICatalogSpecificationRepository repository)
    : IQueryHandler<GetAdminProductVariantsQuery, ICollection<ProductVariantModel>>
{
    public async Task<Result<ICollection<ProductVariantModel>>> ExecuteAsync(
        GetAdminProductVariantsQuery query,
        CancellationToken cancellationToken)
    {
        Product? product = await repository.GetProductAsync(query.ProductId, cancellationToken);
        return product is null
            ? Result.Fail<ICollection<ProductVariantModel>>(new NotFoundError(
                $"Product with id {query.ProductId} not found."))
            : Result.Ok<ICollection<ProductVariantModel>>(product.Variants
                .OrderByDescending(variant => variant.IsDefault)
                .ThenByDescending(variant => variant.IsActive)
                .ThenBy(variant => variant.Sku)
                .Select(variant => variant.ToModel())
                .ToList());
    }
}

public sealed record GetAdminProductVariantsQuery(Guid ProductId);
