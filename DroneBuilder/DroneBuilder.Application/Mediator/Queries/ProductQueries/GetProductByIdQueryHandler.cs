using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Queries.ProductQueries;

public class GetProductByIdQueryHandler(
    IProductRepository productRepository,
    IWarehouseRepository warehouseRepository,
    IMapper mapper)
    : IQueryHandler<GetProductByIdQuery, ProductModel>
{
    public async Task<ProductModel> ExecuteAsync(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetProductByIdAsync(query.ProductId, cancellationToken);

        if (product is null)
        {
            throw new NotFoundException($"Product with id {query.ProductId} not found.");
        }

        var model = mapper.Map<ProductModel>(product);

        var warehouseItem = await warehouseRepository.GetWarehouseItemByProductIdAsync(product.Id, cancellationToken);
        if (warehouseItem != null)
        {
            model.StockQuantity = warehouseItem.Quantity;
        }

        return model;
    }
}

public record GetProductByIdQuery(Guid ProductId);