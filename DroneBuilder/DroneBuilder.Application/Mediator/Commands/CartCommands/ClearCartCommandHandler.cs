using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.Validation;

namespace DroneBuilder.Application.Mediator.Commands.CartCommands;

public class ClearCartCommandHandler(ICartRepository cartRepository, IWarehouseRepository warehouseRepository)
    : ICommandHandler<ClearCartCommand>
{
    public async Task ExecuteCommandAsync(ClearCartCommand command, CancellationToken cancellationToken)
    {
        var cart = await cartRepository.GetCartByUserIdAsync(command.UserId, cancellationToken);

        if (cart == null)
        {
            throw new NotFoundException($"Cart for user ID {command.UserId} not found.");
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

            if (cartItem.Quantity > warehouseItem.ReservedQuantity)
            {
                throw new ValidationException(
                    $"Cart reserved quantity ({cartItem.Quantity}) is greater than warehouse reserved ({warehouseItem.ReservedQuantity}).");
            }

            warehouseItem.ReservedQuantity -= cartItem.Quantity;
            warehouseItem.AvailableQuantity = warehouseItem.Quantity - warehouseItem.ReservedQuantity;

            WarehouseValidation.ValidateState(warehouseItem);
        }

        await cartRepository.ClearCartAsync(cart.Id, cancellationToken);

        await cartRepository.SaveChangesAsync(cancellationToken);
        await warehouseRepository.SaveChangesAsync(cancellationToken);
    }
}

public record ClearCartCommand(Guid UserId);