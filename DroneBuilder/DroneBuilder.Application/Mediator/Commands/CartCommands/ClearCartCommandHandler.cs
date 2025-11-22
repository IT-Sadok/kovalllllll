using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;

namespace DroneBuilder.Application.Mediator.Commands.CartCommands;

public class ClearCartCommandHandler(ICartRepository cartRepository)
    : ICommandHandler<ClearCartCommand>
{
    public async Task ExecuteCommandAsync(ClearCartCommand command, CancellationToken cancellationToken)
    {
        var cart = await cartRepository.GetCartByUserIdAsync(command.UserId, cancellationToken);

        if (cart == null)
        {
            throw new NotFoundException($"Cart for User ID {command.UserId} not found.");
        }

        await cartRepository.ClearCartAsync(cart.Id, cancellationToken);
        await cartRepository.SaveChangesAsync(cancellationToken);
    }
}

public record ClearCartCommand(Guid UserId);