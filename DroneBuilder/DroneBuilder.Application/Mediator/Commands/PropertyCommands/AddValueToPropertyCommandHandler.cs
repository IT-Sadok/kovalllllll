using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;

namespace DroneBuilder.Application.Mediator.Commands.PropertyCommands;

public class AddValueToPropertyCommandHandler(IPropertyRepository propertyRepository, IValueRepository valueRepository)
    :
        ICommandHandler<AddValueToPropertyCommand>
{
    public async Task ExecuteCommandAsync(AddValueToPropertyCommand command, CancellationToken cancellationToken)
    {
        var property = await propertyRepository.GetPropertyByIdAsync(command.PropertyId, cancellationToken);

        if (property == null)
        {
            throw new NotFoundException($"Property with ID {command.PropertyId} not found.");
        }

        var value = await valueRepository.GetValueByIdAsync(command.ValueId, cancellationToken);
        if (value == null)
        {
            throw new NotFoundException($"Value with ID {command.ValueId} not found.");
        }

        if (property.Values != null && property.Values.Any(v => v.Id == command.ValueId))
        {
            throw new ValidationException(
                $"Value with ID {command.ValueId} is already associated with Property ID {command.PropertyId}.");
        }

        property.Values?.Add(value);
        await propertyRepository.SaveChangesAsync(cancellationToken);
    }
}

public record AddValueToPropertyCommand(Guid PropertyId, Guid ValueId);