using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;

namespace DroneBuilder.Application.Mediator.Commands.CartCommands;

public class RemoveItemFromCartCommandHandler(ICartRepository cartRepository, IProductRepository productRepository)
    : ICommandHandler<RemoveItemFromCartCommand>
{
    public async Task ExecuteCommandAsync(RemoveItemFromCartCommand command, CancellationToken cancellationToken)
    {
        var cart = await cartRepository.GetCartByUserIdAsync(command.UserId, cancellationToken);

        if (cart == null)
        {
            throw new NotFoundException($"Cart for User ID {command.UserId} not found.");
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

        await cartRepository.RemoveCartItemAsync(cartItem.Id, cancellationToken);
        await cartRepository.SaveChangesAsync(cancellationToken);
    }
}

public record RemoveItemFromCartCommand(Guid UserId, Guid ProductId);