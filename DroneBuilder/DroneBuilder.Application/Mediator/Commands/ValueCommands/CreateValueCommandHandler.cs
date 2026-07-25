using DroneBuilder.Application.Mappings;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;
namespace DroneBuilder.Application.Mediator.Commands.ValueCommands;

public class CreateValueCommandHandler(
    IValueRepository valueRepository,
    IPropertyRepository propertyRepository) :
    ICommandHandler<CreateValueCommand, ValueModel>
{
    public async Task<Result<ValueModel>> ExecuteCommandAsync(CreateValueCommand command,
        CancellationToken cancellationToken)
    {
        Property? property = await propertyRepository.GetPropertyByIdAsync(command.Model.PropertyId, cancellationToken);
        if (property == null)
        {
            return Result.Fail<ValueModel>(new NotFoundError($"Property with ID {command.Model.PropertyId} not found"));
        }

        if (property.DataType != SpecificationDataType.Option)
        {
            return Result.Fail<ValueModel>(new ValidationError(
                $"Property with ID {property.Id} does not accept predefined option values."));
        }

        Value value = command.Model.ToEntity();

        await valueRepository.AddValueAsync(value, cancellationToken);

        // Link value to property
        property.Values.Add(value);

        await valueRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok(value.ToModel());
    }
}

public record CreateValueCommand(CreateValueModel Model);
