using DroneBuilder.Application.Mappings;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Products.GetAdminProductById;

public sealed class GetAdminProductByIdQueryHandler(IProductRepository repository)
    : IQueryHandler<GetAdminProductByIdQuery, ProductModel>
{
    public async Task<Result<ProductModel>> ExecuteAsync(
        GetAdminProductByIdQuery query,
        CancellationToken cancellationToken)
    {
        Product? product = await repository.GetProductForAdministrationAsync(
            query.ProductId,
            cancellationToken);
        return product is null
            ? Result.Fail<ProductModel>(new NotFoundError($"Product with id {query.ProductId} not found."))
            : Result.Ok(product.ToModel());
    }
}

public sealed record GetAdminProductByIdQuery(Guid ProductId);
