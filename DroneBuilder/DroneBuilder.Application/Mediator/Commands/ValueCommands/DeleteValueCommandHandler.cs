using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Mediator.Commands.ValueCommands;

public class DeleteValueCommandHandler(IValueRepository valueRepository) : ICommandHandler<DeleteValueCommand>
{
    public async Task<Result> ExecuteCommandAsync(DeleteValueCommand command, CancellationToken cancellationToken)
    {
        Value? value = await valueRepository.GetValueByIdAsync(command.ValueId, cancellationToken);
        if (value is null)
        {
            return Result.Fail(new NotFoundError($"Value with id {command.ValueId} not found."));
        }

        valueRepository.RemoveValue(value);
        await valueRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}

public record DeleteValueCommand(Guid ValueId);
