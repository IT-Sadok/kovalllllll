using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;

namespace DroneBuilder.Application.Mediator.Commands.ProductCommands;

public class AddPropertyToProductCommandHandler(
    IProductRepository productRepository,
    IPropertyRepository propertyRepository)
    : ICommandHandler<AddPropertyToProductCommand>
{
    public async Task ExecuteCommandAsync(AddPropertyToProductCommand command, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetProductByIdAsync(command.ProductId, cancellationToken);

        if (product == null)
        {
            throw new NotFoundException($"Product with ID {command.ProductId} not found.");
        }

        var property = await propertyRepository.GetPropertyByIdAsync(command.PropertyId, cancellationToken);

        if (property == null)
        {
            throw new NotFoundException($"Property with ID {command.PropertyId} not found.");
        }

        if (product.Properties != null && product.Properties.Any(p => p.Id == command.PropertyId))
        {
            throw new ValidationException($"Property with ID {command.PropertyId} is already" +
                                          $" associated with Product ID {command.ProductId}.");
        }

        product.Properties?.Add(property);
        await productRepository.SaveChangesAsync(cancellationToken);
    }
}

public record AddPropertyToProductCommand(Guid ProductId, Guid PropertyId);