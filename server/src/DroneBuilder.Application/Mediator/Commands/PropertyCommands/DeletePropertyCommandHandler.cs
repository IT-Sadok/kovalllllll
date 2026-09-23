using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Mediator.Commands.PropertyCommands;

public class DeletePropertyCommandHandler(IPropertyRepository propertyRepository)
    : ICommandHandler<DeletePropertyCommand>
{
    public async Task<Result> ExecuteCommandAsync(DeletePropertyCommand command, CancellationToken cancellationToken)
    {
        Property? property = await propertyRepository.GetPropertyByIdAsync(command.PropertyId, cancellationToken);

        if (property is null)
        {
            return Result.Fail(new NotFoundError($"Property with id {command.PropertyId} not found."));
        }

        propertyRepository.RemoveProperty(property);
        await propertyRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}

public record DeletePropertyCommand(Guid PropertyId);
