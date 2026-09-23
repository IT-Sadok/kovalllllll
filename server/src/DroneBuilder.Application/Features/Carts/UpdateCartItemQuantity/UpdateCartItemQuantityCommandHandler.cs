using DroneBuilder.Application.Common.Abstractions;
using DroneBuilder.Application.Common.Contexts;
using DroneBuilder.Application.Common.Mediator.Interfaces;
using DroneBuilder.Application.Common.Options;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Application.Common.ResultErrors;
using DroneBuilder.Application.Features.Warehouses;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Events.CartEvents;
using FluentResults;
namespace DroneBuilder.Application.Features.Carts.UpdateCartItemQuantity;

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
