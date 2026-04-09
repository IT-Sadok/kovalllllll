using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;

namespace DroneBuilder.Application.Mediator.Commands.ProductCommands;

public class RemoveValueFromProductPropertyCommandHandler(IProductRepository productRepository)
    : ICommandHandler<RemoveValueFromProductPropertyCommand>
{
    public async Task ExecuteCommandAsync(RemoveValueFromProductPropertyCommand command, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetProductByIdAsync(command.ProductId, cancellationToken);
        if (product == null) throw new NotFoundException($"Product with ID {command.ProductId} not found.");

        if (product.ProductPropertyValues != null)
        {
            var item = product.ProductPropertyValues.FirstOrDefault(p => p.PropertyId == command.PropertyId && p.ValueId == command.ValueId);
            if (item != null)
            {
                product.ProductPropertyValues.Remove(item);
                await productRepository.SaveChangesAsync(cancellationToken);
            }
        }
    }
}

public record RemoveValueFromProductPropertyCommand(Guid ProductId, Guid PropertyId, Guid ValueId);
