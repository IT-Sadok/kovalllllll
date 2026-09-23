using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Properties.RemoveValueFromProperty;

public class RemoveValueFromPropertyCommandHandler(
    IPropertyRepository propertyRepository,
    IValueRepository valueRepository)
    : ICommandHandler<RemoveValueFromPropertyCommand>
{
    public async Task<Result> ExecuteCommandAsync(RemoveValueFromPropertyCommand command, CancellationToken cancellationToken)
    {
        Property? property = await propertyRepository.GetPropertyByIdAsync(command.PropertyId, cancellationToken);
        if (property == null)
        {
            return Result.Fail(new NotFoundError($"Property with ID {command.PropertyId} not found."));
        }

        Value? value = await valueRepository.GetValueByIdAsync(command.ValueId, cancellationToken);
        if (value == null)
        {
            return Result.Fail(new NotFoundError($"Value with ID {command.ValueId} not found."));
        }

        if (property.Values.Contains(value))
        {
            property.Values.Remove(value);

            await propertyRepository.RemoveProductAssignmentsAsync(command.PropertyId, command.ValueId, cancellationToken);
            await propertyRepository.SaveChangesAsync(cancellationToken);

            Value? updatedValue = await valueRepository.GetValueWithPropertiesByIdAsync(command.ValueId, cancellationToken);
            if (updatedValue != null && (updatedValue.Properties == null || updatedValue.Properties.Count == 0))
            {
                valueRepository.RemoveValue(updatedValue);
                await valueRepository.SaveChangesAsync(cancellationToken);
            }
        }

        return Result.Ok();
    }
}

public record RemoveValueFromPropertyCommand(Guid PropertyId, Guid ValueId);
