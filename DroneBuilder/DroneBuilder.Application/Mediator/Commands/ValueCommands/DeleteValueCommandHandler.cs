using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;

namespace DroneBuilder.Application.Mediator.Commands.ValueCommands;

public class DeleteValueCommandHandler(IValueRepository valueRepository) : ICommandHandler<DeleteValueCommand>
{
    public async Task ExecuteCommandAsync(DeleteValueCommand command, CancellationToken cancellationToken)
    {
        var value = valueRepository.GetValueByIdAsync(command.ValueId, cancellationToken).Result;
        if (value is null)
        {
            throw new NotFoundException($"Value with id {command.ValueId} not found.");
        }

        valueRepository.RemoveValue(value);
        await valueRepository.SaveChangesAsync(cancellationToken);
    }
}

public record DeleteValueCommand(Guid ValueId);