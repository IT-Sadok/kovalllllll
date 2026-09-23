using DroneBuilder.Application.Common.Abstractions;
using DroneBuilder.Application.Common.Contexts;
using DroneBuilder.Application.Common.Errors;
using DroneBuilder.Application.Common.Mediator.Interfaces;
using DroneBuilder.Application.Common.Options;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Application.Features.Warehouses;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Events.CartEvents;
using FluentResults;
namespace DroneBuilder.Application.Features.Carts.RemoveItemFromCart;

public class RemoveItemFromCartCommandHandler(
    ICartRepository cartRepository,
    IProductRepository productRepository,
    IWarehouseRepository warehouseRepository,
    IOutboxEventService outboxService,
    MessageQueuesConfiguration queuesConfig,
    IUserContext userContext)
    : ICommandHandler<RemoveItemFromCartCommand>
{
    public async Task<Result> ExecuteCommandAsync(RemoveItemFromCartCommand command, CancellationToken cancellationToken)
    {
        Cart? cart = await cartRepository.GetCartByUserIdAsync(userContext.UserId, cancellationToken);

        if (cart == null)
        {
            return Result.Fail(new NotFoundError($"Cart for User ID {userContext.UserId} not found."));
        }

        Product? product = await productRepository.GetProductByIdAsync(command.ProductId, cancellationToken);

        if (product == null)
        {
            return Result.Fail(new NotFoundError($"Product with ID {command.ProductId} not found."));
        }

        CartItem? cartItem = cart.CartItems.FirstOrDefault(item => item.ProductId == command.ProductId);

        if (cartItem == null)
        {
            return Result.Fail(new NotFoundError($"Product with ID {command.ProductId} not found in the cart."));
        }

        WarehouseItem? warehouseItem =
            await warehouseRepository.GetWarehouseItemByProductIdAsync(command.ProductId, cancellationToken);

        if (warehouseItem == null)
        {
            return Result.Fail(new NotFoundError($"Warehouse item for Product ID {command.ProductId} not found."));
        }

        Result validationResult = WarehouseValidation.ValidateState(warehouseItem);
        if (validationResult.IsFailed)
        {
            return validationResult;
        }

        warehouseItem.Quantity += cartItem.Quantity;

        validationResult = WarehouseValidation.ValidateState(warehouseItem);
        if (validationResult.IsFailed)
        {
            return validationResult;
        }

        await cartRepository.RemoveCartItemAsync(cartItem.Id, cancellationToken);

        var @event = new UpdatedCartItemQuantityEvent(userContext.UserId, command.ProductId, 0);
        await outboxService.StoreEventAsync(@event, queuesConfig.CartQueue.Name, cancellationToken);

        await cartRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}

public record RemoveItemFromCartCommand(Guid ProductId);
