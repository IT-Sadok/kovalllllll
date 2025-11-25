using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;

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
            if (warehouseItem != null)
            {
                warehouseItem.ReservedQuantity -= cartItem.Quantity;
                warehouseItem.AvailableQuantity = warehouseItem.Quantity - warehouseItem.ReservedQuantity;
            }
        }

        await cartRepository.ClearCartAsync(cart.Id, cancellationToken);

        await cartRepository.SaveChangesAsync(cancellationToken);
        await warehouseRepository.SaveChangesAsync(cancellationToken);
    }
}

public record ClearCartCommand(Guid UserId);