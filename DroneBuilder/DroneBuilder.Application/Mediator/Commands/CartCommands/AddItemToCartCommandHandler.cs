using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Commands.CartCommands;

public class AddItemToCartCommandHandler(
    ICartRepository cartRepository,
    IProductRepository productRepository,
    IMapper mapper) : ICommandHandler<AddItemToCartCommand>
{
    public async Task ExecuteCommandAsync(AddItemToCartCommand command,
        CancellationToken cancellationToken)
    {
        if (command.Quantity <= 0)
        {
            throw new BadRequestException("Quantity must be greater than zero.");
        }

        var existingProduct = await productRepository.GetProductByIdAsync(command.ProductId, cancellationToken);
        if (existingProduct == null)
        {
            throw new NotFoundException($"Product with ID {command.ProductId} not found.");
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


        if (existingCartItem != null)
        {
            existingCartItem.Quantity += command.Quantity;
        }
        else
        {
            var newCartItem = new CartItem
            {
                ProductId = command.ProductId,
                Quantity = command.Quantity,
                Cart = cart
            };
            cart.CartItems.Add(newCartItem);
        }

        await cartRepository.SaveChangesAsync(cancellationToken);
    }
}

public record AddItemToCartCommand(Guid UserId, Guid ProductId, int Quantity);