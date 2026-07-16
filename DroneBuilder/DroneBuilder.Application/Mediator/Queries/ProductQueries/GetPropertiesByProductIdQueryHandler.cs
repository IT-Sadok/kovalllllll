using DroneBuilder.Application.Mappings;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;
namespace DroneBuilder.Application.Mediator.Queries.ProductQueries;

public class GetPropertiesByProductIdQueryHandler(
    IProductRepository productRepository,
    IWarehouseRepository warehouseRepository)
    : IQueryHandler<GetPropertiesByProductIdQuery, ProductPropertiesResponseModel>
{
    public async Task<Result<ProductPropertiesResponseModel>> ExecuteAsync(GetPropertiesByProductIdQuery query,
        CancellationToken cancellationToken)
    {
        Product? product = await productRepository.GetPropertiesByProductIdAsync(query.ProductId, cancellationToken);

        if (product is null)
        {
            return Result.Fail<ProductPropertiesResponseModel>(new NotFoundError($"Product with id {query.ProductId} not found."));
        }

        ProductPropertiesResponseModel model = product.ToPropertiesResponseModel();

        WarehouseItem? warehouseItem = await warehouseRepository.GetWarehouseItemByProductIdAsync(product.Id, cancellationToken);
        if (warehouseItem != null)
        {
            model.StockQuantity = warehouseItem.Quantity;
        }

        return Result.Ok(model);
    }
}

public record GetPropertiesByProductIdQuery(Guid ProductId);
