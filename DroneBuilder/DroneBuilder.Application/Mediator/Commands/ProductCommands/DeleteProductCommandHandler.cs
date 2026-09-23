using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Mediator.Commands.ProductCommands;

public class DeleteProductCommandHandler(
    IProductRepository productRepository,
    ICartRepository cartRepository,
    IWarehouseRepository warehouseRepository)
    : ICommandHandler<DeleteProductCommand>
{
    public async Task<Result> ExecuteCommandAsync(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        Product? existingProduct = await productRepository.GetProductByIdAsync(command.ProductId, cancellationToken);
        if (existingProduct is null)
        {
            return Result.Fail(new NotFoundError($"Product with id {command.ProductId} not found."));
        }

        existingProduct.IsDeleted = true;

        int releasedQuantity =
            await cartRepository.RemoveCartItemsByProductIdAsync(command.ProductId, cancellationToken);

        WarehouseItem? warehouseItem =
            await warehouseRepository.GetWarehouseItemByProductIdAsync(command.ProductId, cancellationToken);

        if (warehouseItem is not null)
        {
            warehouseItem.Quantity += releasedQuantity;
        }

        await productRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}

public record DeleteProductCommand(Guid ProductId);
