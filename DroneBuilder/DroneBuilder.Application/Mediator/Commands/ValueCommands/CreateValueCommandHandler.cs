using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Commands.ValueCommands;

public class CreateValueCommandHandler(
    IValueRepository valueRepository,
    IPropertyRepository propertyRepository,
    IMapper mapper) :
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

        Value value = mapper.Map<Value>(command.Model);

        await valueRepository.AddValueAsync(value, cancellationToken);

        // Link value to property
        property.Values.Add(value);

        await valueRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok(mapper.Map<ValueModel>(value));
    }
}

public record CreateValueCommand(CreateValueModel Model);
