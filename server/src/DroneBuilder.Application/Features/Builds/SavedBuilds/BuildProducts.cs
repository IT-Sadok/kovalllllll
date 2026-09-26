using DroneBuilder.Application.Common.Errors;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Application.Features.Builds.CheckBuild;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Builds.SavedBuilds;

public static class BuildProducts
{
    public static async Task<Result> EnsureExistAsync(IProductRepository productRepository,
        IEnumerable<BuildItemModel> items, CancellationToken cancellationToken)
    {
        List<Guid> ids = items.Select(i => i.ProductId).ToList();
        ICollection<Product> products = await productRepository.GetProductsByIdsAsync(ids, cancellationToken);

        List<Guid> unknown = ids.Except(products.Select(p => p.Id)).ToList();
        return unknown.Count == 0
            ? Result.Ok()
            : Result.Fail(new NotFoundError($"Products not found: {string.Join(", ", unknown)}."));
    }
}
