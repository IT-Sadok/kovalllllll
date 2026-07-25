using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Properties.AddValueToProperty;

public class AddValueToPropertyCommandHandler(IPropertyRepository propertyRepository, IValueRepository valueRepository)
    :
        ICommandHandler<AddValueToPropertyCommand>
{
    public async Task<Result> ExecuteCommandAsync(AddValueToPropertyCommand command, CancellationToken cancellationToken)
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

        if (property.DataType != SpecificationDataType.Option)
        {
            return Result.Fail(new ValidationError(
                $"Property with ID {property.Id} does not accept predefined option values."));
        }

        if (property.Values != null && property.Values.Any(v => v.Id == command.ValueId))
        {
            return Result.Fail(new ValidationError(
                $"Value with ID {command.ValueId} is already associated with Property ID {command.PropertyId}."));
        }

        property.Values?.Add(value);
        await propertyRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}

public record AddValueToPropertyCommand(Guid PropertyId, Guid ValueId);
