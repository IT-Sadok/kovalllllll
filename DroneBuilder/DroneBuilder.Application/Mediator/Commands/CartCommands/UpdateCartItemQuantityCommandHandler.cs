using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Contexts;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Options;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Application.Validation;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Events.CartEvents;
using FluentResults;

namespace DroneBuilder.Application.Mediator.Commands.CartCommands;

public class UpdateCartItemQuantityCommandHandler(
    ICartRepository cartRepository,
    IWarehouseRepository warehouseRepository,
    IOutboxEventService outboxService,
    MessageQueuesConfiguration queuesConfig,
    IUserContext userContext)
    : ICommandHandler<UpdateCartItemQuantityCommand>
{
    public async Task<Result> ExecuteCommandAsync(UpdateCartItemQuantityCommand command, CancellationToken cancellationToken)
    {
        Cart? cart = await cartRepository.GetCartByUserIdAsync(userContext.UserId, cancellationToken);
        if (cart == null)
        {
            return Result.Fail(new NotFoundError($"Cart for user with ID {userContext.UserId} not found."));
        }

        CartItem? cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == command.ProductId);
        if (cartItem == null)
        {
            return Result.Fail(new NotFoundError($"Product with ID {command.ProductId} not found in cart."));
        }

        WarehouseItem? warehouseItem = await warehouseRepository.GetWarehouseItemByProductIdAsync(command.ProductId, cancellationToken);
        if (warehouseItem == null)
        {
            return Result.Fail(new NotFoundError($"Warehouse item for product ID {command.ProductId} not found."));
        }

        int quantityDifference = command.Quantity - cartItem.Quantity;

        if (quantityDifference == 0)
        {
            return Result.Ok();
        }

        // Only an increase takes stock out of the warehouse; a decrease puts it back.
        if (quantityDifference > 0)
        {
            Result availabilityResult = WarehouseValidation.EnsureEnoughAvailable(warehouseItem, quantityDifference);
            if (availabilityResult.IsFailed)
            {
                return availabilityResult;
            }
        }

        warehouseItem.Quantity -= quantityDifference;

        if (command.Quantity == 0)
        {
            await cartRepository.RemoveCartItemAsync(cartItem.Id, cancellationToken);
        }
        else
        {
            cartItem.Quantity = command.Quantity;

            // Only growing the item takes new stock out, so only that restarts the reservation.
            if (quantityDifference > 0)
            {
                cartItem.ReservedAt = DateTime.UtcNow;
            }
        }

        var @event = new UpdatedCartItemQuantityEvent(userContext.UserId, command.ProductId, command.Quantity);
        await outboxService.StoreEventAsync(@event, queuesConfig.CartQueue.Name, cancellationToken);

        await cartRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}

public record UpdateCartItemQuantityCommand(Guid ProductId, int Quantity);
