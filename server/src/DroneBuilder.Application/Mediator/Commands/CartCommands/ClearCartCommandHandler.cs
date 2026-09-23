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

public class ClearCartCommandHandler(
    ICartRepository cartRepository,
    IWarehouseRepository warehouseRepository,
    IOutboxEventService outboxService,
    MessageQueuesConfiguration queuesConfig,
    IUserContext userContext)
    : ICommandHandler<ClearCartCommand>
{
    public async Task<Result> ExecuteCommandAsync(ClearCartCommand command, CancellationToken cancellationToken)
    {
        Cart? cart = await cartRepository.GetCartByUserIdAsync(userContext.UserId, cancellationToken);

        if (cart == null)
        {
            return Result.Fail(new NotFoundError($"Cart for user ID {userContext.UserId} not found."));
        }

        foreach (CartItem cartItem in cart.CartItems)
        {
            WarehouseItem? warehouseItem =
                await warehouseRepository.GetWarehouseItemByProductIdAsync(cartItem.ProductId, cancellationToken);

            if (warehouseItem == null)
            {
                return Result.Fail(new NotFoundError(
                    $"Warehouse item for product {cartItem.ProductId} not found while clearing cart."));
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
        }

        await cartRepository.ClearCartAsync(cart.Id, cancellationToken);

        var @event = new ClearedCartEvent(userContext.UserId);
        await outboxService.StoreEventAsync(@event, queuesConfig.CartQueue.Name, cancellationToken);

        await cartRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}

public record ClearCartCommand();
