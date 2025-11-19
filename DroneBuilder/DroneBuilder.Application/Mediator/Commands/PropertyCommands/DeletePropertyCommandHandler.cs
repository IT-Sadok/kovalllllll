using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Mediator.Commands.PropertyCommands;

public class DeletePropertyCommandHandler(IPropertyRepository propertyRepository)
    : ICommandHandler<DeletePropertyCommand>
{
    public async Task ExecuteCommandAsync(DeletePropertyCommand command, CancellationToken cancellationToken)
    {
        var property = await propertyRepository.GetPropertyByIdAsync(command.PropertyId, cancellationToken);

        if (property is null)
        {
            throw new NotFoundException($"Property with id {command.PropertyId} not found.");
        }

        propertyRepository.RemoveProperty(property);
        await propertyRepository.SaveChangesAsync(cancellationToken);
    }
}

public record DeletePropertyCommand(Guid PropertyId);