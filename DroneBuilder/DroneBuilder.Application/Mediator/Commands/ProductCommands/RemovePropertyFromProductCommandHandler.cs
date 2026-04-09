using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;

namespace DroneBuilder.Application.Mediator.Commands.ProductCommands;

public class RemovePropertyFromProductCommandHandler(IProductRepository productRepository)
    : ICommandHandler<RemovePropertyFromProductCommand>
{
    public async Task ExecuteCommandAsync(RemovePropertyFromProductCommand command, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetProductByIdAsync(command.ProductId, cancellationToken);
        if (product == null) throw new NotFoundException($"Product with ID {command.ProductId} not found.");

        if (product.ProductPropertyValues != null)
        {
            var itemsToRemove = product.ProductPropertyValues.Where(p => p.PropertyId == command.PropertyId).ToList();
            if (itemsToRemove.Any())
            {
                foreach(var item in itemsToRemove)
                {
                    product.ProductPropertyValues.Remove(item);
                }
                await productRepository.SaveChangesAsync(cancellationToken);
            }
        }
    }
}

public record RemovePropertyFromProductCommand(Guid ProductId, Guid PropertyId);
