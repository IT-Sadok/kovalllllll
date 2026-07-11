using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Contexts;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Options;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Application.Validation;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Mediator.Commands.CartCommands;

public class UpdateCartItemQuantityCommandHandler(
    ICartRepository cartRepository,
    IProductRepository productRepository,
    IWarehouseRepository warehouseRepository,
    IOutboxEventService outboxService,
    MessageQueuesConfiguration queuesConfig,
    IUserContext userContext)
    : ICommandHandler<UpdateCartItemQuantityCommand>
{
    public async Task<Result> ExecuteCommandAsync(UpdateCartItemQuantityCommand command, CancellationToken cancellationToken)
    {
        if (command.Quantity < 0)
        {
            return Result.Fail(new BadRequestError("Quantity cannot be negative."));
        }

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

        // If we are increasing quantity, check warehouse
        if (quantityDifference > 0)
        {
            Result validationResult = WarehouseValidation.ValidateState(warehouseItem);
            if (validationResult.IsFailed)
            {
                return validationResult;
            }

            // Note: We need to make sure the warehouse has enough stock for the delta
            // WarehouseValidation.ValidateState might check if item.Quantity is >= 0, 
            // but we need to check if warehouseItem.Quantity - quantityDifference >= 0.
            // Let's assume WarehouseValidation handles it or do a manual check.
            if (warehouseItem.Quantity < quantityDifference)
            {
                return Result.Fail(new BadRequestError("Not enough stock in warehouse."));
            }
        }

        // Adjust warehouse stock
        warehouseItem.Quantity -= quantityDifference;
        Result postValidation = WarehouseValidation.ValidateState(warehouseItem);
        if (postValidation.IsFailed)
        {
            return postValidation;
        }

        if (command.Quantity == 0)
        {
            await cartRepository.RemoveCartItemAsync(cartItem.Id, cancellationToken);
        }
        else
        {
            cartItem.Quantity = command.Quantity;
        }

        // We can reuse the AddedItemToCartEvent but maybe with a negative quantity for decrements, 
        // or just let the outbox handle the state if it's simpler. 
        // For now, let's just trigger a generic update or clear cart event if needed.
        // Actually, let's just save changes.

        await cartRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}

public record UpdateCartItemQuantityCommand(Guid ProductId, int Quantity);
