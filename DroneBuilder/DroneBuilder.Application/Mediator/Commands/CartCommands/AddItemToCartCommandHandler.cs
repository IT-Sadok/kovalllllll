using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Commands.CartCommands;

public class AddItemToCartCommandHandler(
    ICartRepository cartRepository,
    IWarehouseRepository warehouseRepository,
    IProductRepository productRepository,
    IMapper mapper) : ICommandHandler<AddItemToCartCommand>
{
    public async Task ExecuteCommandAsync(AddItemToCartCommand command,
        CancellationToken cancellationToken)
    {
        var existingProduct = await productRepository.GetProductByIdAsync(command.ProductId, cancellationToken);
        if (existingProduct == null)
        {
            throw new NotFoundException($"Product with ID {command.ProductId} not found.");
        }

        var warehouseItem =
            await warehouseRepository.GetWarehouseItemByProductIdAsync(command.ProductId, cancellationToken);
        if (warehouseItem == null)
        {
            throw new NotFoundException($"Warehouse item for product ID {command.ProductId} not found.");
        }

        if (command.Quantity <= 0)
        {
            throw new BadRequestException("Quantity must be greater than zero.");
        }

        if (warehouseItem.Quantity < command.Quantity)
        {
            throw new BadRequestException("Insufficient stock in warehouse.");
        }


        var cart = await cartRepository.GetCartByUserIdAsync(command.UserId, cancellationToken);

        if (cart == null)
        {
            cart = new Cart
            {
                UserId = command.UserId,
                CartItems = new List<CartItem>()
            };
            await cartRepository.CreateCartAsync(cart, cancellationToken);
        }

        var existingCartItem = cart.CartItems
            .FirstOrDefault(ci => ci.ProductId == command.ProductId);


        if (existingCartItem == null)
        {
            var newCartItem = new CartItem
            {
                ProductId = command.ProductId,
                ProductName = existingProduct.Name,
                Quantity = command.Quantity,
                Cart = cart
            };
            await cartRepository.AddCartItemAsync(newCartItem, cancellationToken);
        }
        else
        {
            existingCartItem.Quantity += command.Quantity;
        }

        var available = warehouseItem.Quantity - warehouseItem.ReservedQuantity;
        if (command.Quantity > available)
            throw new BadRequestException("Insufficient stock in warehouse.");


        warehouseItem.ReservedQuantity += command.Quantity;

        await cartRepository.SaveChangesAsync(cancellationToken);
    }
}

public record AddItemToCartCommand(Guid UserId, Guid ProductId, int Quantity);