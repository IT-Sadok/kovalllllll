using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Contexts;
using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Options;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.Validation;
using DroneBuilder.Domain.Events.CartEvents;

namespace DroneBuilder.Application.Mediator.Commands.CartCommands;

public class ClearCartCommandHandler(
    ICartRepository cartRepository,
    IWarehouseRepository warehouseRepository,
    IOutboxEventService outboxService,
    MessageQueuesConfiguration queuesConfig,
    IUserContext userContext)
    : ICommandHandler<ClearCartCommand>
{
    public async Task ExecuteCommandAsync(ClearCartCommand command, CancellationToken cancellationToken)
    {
        var cart = await cartRepository.GetCartByUserIdAsync(userContext.UserId, cancellationToken);

        if (cart == null)
        {
            throw new NotFoundException($"Cart for user ID {userContext.UserId} not found.");
        }


        foreach (var cartItem in cart.CartItems)
        {
            var warehouseItem =
                await warehouseRepository.GetWarehouseItemByProductIdAsync(cartItem.ProductId, cancellationToken);

            if (warehouseItem == null)
            {
                throw new NotFoundException(
                    $"Warehouse item for product {cartItem.ProductId} not found while clearing cart.");
            }

            WarehouseValidation.ValidateState(warehouseItem);

            warehouseItem.Quantity += cartItem.Quantity;

            WarehouseValidation.ValidateState(warehouseItem);
        }

        await cartRepository.ClearCartAsync(cart.Id, cancellationToken);

        var @event = new ClearedCartEvent(userContext.UserId);
        await outboxService.StoreEventAsync(@event, queuesConfig.CartQueue.Name, cancellationToken);

        await cartRepository.SaveChangesAsync(cancellationToken);
    }
}

public record ClearCartCommand();