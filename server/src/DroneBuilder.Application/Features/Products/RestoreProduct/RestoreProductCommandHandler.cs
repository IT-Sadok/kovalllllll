using DroneBuilder.Application.Common.Mediator.Interfaces;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Application.Common.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Products.RestoreProduct;

public class RestoreProductCommandHandler(
    IProductRepository productRepository,
    IWarehouseRepository warehouseRepository)
    : ICommandHandler<RestoreProductCommand>
{
    public async Task<Result> ExecuteCommandAsync(RestoreProductCommand command, CancellationToken cancellationToken)
    {
        Product? product = await productRepository.GetDelistedProductByIdAsync(command.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Fail(new NotFoundError($"Delisted product with id {command.ProductId} not found."));
        }

        Warehouse? warehouse = await warehouseRepository.GetWarehouseAsync(cancellationToken);
        if (warehouse is null)
        {
            return Result.Fail(new NotFoundError("Warehouse not found."));
        }

        product.IsDeleted = false;

        WarehouseItem? warehouseItem =
            await warehouseRepository.GetWarehouseItemByProductIdAsync(command.ProductId, cancellationToken);

        if (warehouseItem is null)
        {
            await warehouseRepository.AddWarehouseItemAsync(
                new WarehouseItem
                {
                    WarehouseId = warehouse.Id,
                    ProductId = product.Id
                },
                cancellationToken);
        }

        await productRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}

public record RestoreProductCommand(Guid ProductId);
