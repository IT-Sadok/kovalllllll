using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Mediator.Commands.ProductCommands;

public class AddValueToProductPropertyCommandHandler(
    IProductRepository productRepository,
    IPropertyRepository propertyRepository,
    IValueRepository valueRepository)
    : ICommandHandler<AddValueToProductPropertyCommand>
{
    public async Task ExecuteCommandAsync(AddValueToProductPropertyCommand command, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetProductByIdAsync(command.ProductId, cancellationToken);
        if (product == null)
            throw new NotFoundException($"Product with ID {command.ProductId} not found.");

        var property = await propertyRepository.GetPropertyByIdAsync(command.PropertyId, cancellationToken);
        if (property == null)
            throw new NotFoundException($"Property with ID {command.PropertyId} not found.");

        var value = await valueRepository.GetValueByIdAsync(command.ValueId, cancellationToken);
        if (value == null)
            throw new NotFoundException($"Value with ID {command.ValueId} not found.");

        if (product.ProductPropertyValues != null && 
            product.ProductPropertyValues.Any(p => p.PropertyId == command.PropertyId && p.ValueId == command.ValueId))
        {
            throw new ValidationException($"Value with ID {command.ValueId} is already associated with Property ID {command.PropertyId} on Product ID {command.ProductId}.");
        }

        product.ProductPropertyValues?.Add(new ProductPropertyValue
        {
            ProductId = product.Id,
            PropertyId = property.Id,
            ValueId = value.Id
        });
        
        await productRepository.SaveChangesAsync(cancellationToken);
    }
}

public record AddValueToProductPropertyCommand(Guid ProductId, Guid PropertyId, Guid ValueId);