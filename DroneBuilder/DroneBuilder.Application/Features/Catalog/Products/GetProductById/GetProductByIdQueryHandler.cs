using DroneBuilder.Application.Mappings;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Products.GetProductById;

public class GetProductByIdQueryHandler(
    IProductRepository productRepository,
    IWarehouseRepository warehouseRepository)
    : IQueryHandler<GetProductByIdQuery, ProductModel>
{
    public async Task<Result<ProductModel>> ExecuteAsync(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        Product? product = await productRepository.GetProductByIdAsync(query.ProductId, cancellationToken);

        if (product is null || product.PublicationStatus != ProductPublicationStatus.Published)
        {
            return Result.Fail<ProductModel>(new NotFoundError($"Product with id {query.ProductId} not found."));
        }

        ProductModel model = product.ToModel();

        WarehouseItem? warehouseItem = await warehouseRepository.GetWarehouseItemByProductIdAsync(product.Id, cancellationToken);
        if (warehouseItem != null)
        {
            model.StockQuantity = warehouseItem.AvailableQuantity;
        }

        return Result.Ok(model);
    }
}

public record GetProductByIdQuery(Guid ProductId);
