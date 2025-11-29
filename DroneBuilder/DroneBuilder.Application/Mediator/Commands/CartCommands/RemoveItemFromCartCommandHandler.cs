using DroneBuilder.Application.Contexts;
using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.Validation;

namespace DroneBuilder.Application.Mediator.Commands.CartCommands;

public class RemoveItemFromCartCommandHandler(
    ICartRepository cartRepository,
    IProductRepository productRepository,
    IWarehouseRepository warehouseRepository,
    IUserContext userContext)
    : ICommandHandler<RemoveItemFromCartCommand>
{
    public async Task ExecuteCommandAsync(RemoveItemFromCartCommand command, CancellationToken cancellationToken)
    {
        var cart = await cartRepository.GetCartByUserIdAsync(userContext.UserId, cancellationToken);

        if (cart == null)
        {
            throw new NotFoundException($"Cart for User ID {userContext.UserId} not found.");
        }

        var product = await productRepository.GetProductByIdAsync(command.ProductId, cancellationToken);

        if (product == null)
        {
            throw new NotFoundException($"Product with ID {command.ProductId} not found.");
        }

        var cartItem = cart.CartItems.FirstOrDefault(item => item.ProductId == command.ProductId);

        if (cartItem == null)
        {
            throw new NotFoundException($"Product with ID {command.ProductId} not found in the cart.");
        }

        var warehouseItem =
            await warehouseRepository.GetWarehouseItemByProductIdAsync(command.ProductId, cancellationToken);

        if (warehouseItem == null)
        {
            throw new NotFoundException($"Warehouse item for Product ID {command.ProductId} not found.");
        }

        WarehouseValidation.ValidateState(warehouseItem);

        warehouseItem.Quantity += cartItem.Quantity;

        WarehouseValidation.ValidateState(warehouseItem);

        await cartRepository.RemoveCartItemAsync(cartItem.Id, cancellationToken);
        await cartRepository.SaveChangesAsync(cancellationToken);
    }
}

public record RemoveItemFromCartCommand(Guid ProductId);