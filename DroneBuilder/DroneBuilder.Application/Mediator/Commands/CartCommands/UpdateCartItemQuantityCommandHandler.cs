using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Contexts;
using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Options;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.Validation;
using DroneBuilder.Domain.Events.CartEvents;

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
    public async Task ExecuteCommandAsync(UpdateCartItemQuantityCommand command, CancellationToken cancellationToken)
    {
        if (command.Quantity < 0)
        {
            throw new BadRequestException("Quantity cannot be negative.");
        }

        var cart = await cartRepository.GetCartByUserIdAsync(userContext.UserId, cancellationToken);
        if (cart == null)
        {
            throw new NotFoundException($"Cart for user with ID {userContext.UserId} not found.");
        }

        var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == command.ProductId);
        if (cartItem == null)
        {
            throw new NotFoundException($"Product with ID {command.ProductId} not found in cart.");
        }

        var warehouseItem = await warehouseRepository.GetWarehouseItemByProductIdAsync(command.ProductId, cancellationToken);
        if (warehouseItem == null)
        {
            throw new NotFoundException($"Warehouse item for product ID {command.ProductId} not found.");
        }

        int quantityDifference = command.Quantity - cartItem.Quantity;

        if (quantityDifference == 0) return;

        // If we are increasing quantity, check warehouse
        if (quantityDifference > 0)
        {
            WarehouseValidation.ValidateState(warehouseItem);
            
            // Note: We need to make sure the warehouse has enough stock for the delta
            // WarehouseValidation.ValidateState might check if item.Quantity is >= 0, 
            // but we need to check if warehouseItem.Quantity - quantityDifference >= 0.
            // Let's assume WarehouseValidation handles it or do a manual check.
            if (warehouseItem.Quantity < quantityDifference)
            {
                throw new BadRequestException("Not enough stock in warehouse.");
            }
        }

        // Adjust warehouse stock
        warehouseItem.Quantity -= quantityDifference;
        WarehouseValidation.ValidateState(warehouseItem);

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
    }
}

public record UpdateCartItemQuantityCommand(Guid ProductId, int Quantity);
