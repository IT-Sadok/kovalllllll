using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Queries.ProductQueries;

public class GetProductByIdQueryHandler(
    IProductRepository productRepository,
    IWarehouseRepository warehouseRepository,
    IMapper mapper)
    : IQueryHandler<GetProductByIdQuery, ProductModel>
{
    public async Task<Result<ProductModel>> ExecuteAsync(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        Product? product = await productRepository.GetProductByIdAsync(query.ProductId, cancellationToken);

        if (product is null)
        {
            return Result.Fail<ProductModel>(new NotFoundError($"Product with id {query.ProductId} not found."));
        }

        ProductModel model = mapper.Map<ProductModel>(product);

        WarehouseItem? warehouseItem = await warehouseRepository.GetWarehouseItemByProductIdAsync(product.Id, cancellationToken);
        if (warehouseItem != null)
        {
            model.StockQuantity = warehouseItem.Quantity;
        }

        return Result.Ok(model);
    }
}

public record GetProductByIdQuery(Guid ProductId);
