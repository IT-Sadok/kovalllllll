using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DroneBuilder.Application.Mediator.Commands.PropertyCommands;

public class RemoveValueFromPropertyCommandHandler(
    IPropertyRepository propertyRepository,
    IValueRepository valueRepository)
    : ICommandHandler<RemoveValueFromPropertyCommand>
{
    public async Task ExecuteCommandAsync(RemoveValueFromPropertyCommand command, CancellationToken cancellationToken)
    {
        var property = await propertyRepository.GetPropertyByIdAsync(command.PropertyId, cancellationToken);
        if (property == null)
            throw new NotFoundException($"Property with ID {command.PropertyId} not found.");

        var value = await valueRepository.GetValueByIdAsync(command.ValueId, cancellationToken);
        if (value == null)
            throw new NotFoundException($"Value with ID {command.ValueId} not found.");

        // Remove the link
        if (property.Values.Contains(value))
        {
            property.Values.Remove(value);
            await propertyRepository.SaveChangesAsync(cancellationToken);
            
            // Re-load value with properties to check for orphans
            // We need to check if this value is still linked to any other properties
            // or if it's used in any ProductPropertyValues.
            
            // However, our rule is "Value exists if it belongs to a property".
            // So we check if it has any properties left.
            
            var updatedValue = await valueRepository.GetValueWithPropertiesByIdAsync(command.ValueId, cancellationToken);
            if (updatedValue != null && (updatedValue.Properties == null || updatedValue.Properties.Count == 0))
            {
                valueRepository.RemoveValue(updatedValue);
                await valueRepository.SaveChangesAsync(cancellationToken);
            }
        }
    }
}

public record RemoveValueFromPropertyCommand(Guid PropertyId, Guid ValueId);
