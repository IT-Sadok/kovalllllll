using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Commands.ValueCommands;

public class CreateValueCommandHandler(
    IValueRepository valueRepository,
    IPropertyRepository propertyRepository,
    IMapper mapper) :
    ICommandHandler<CreateValueCommand, ValueModel>
{
    public async Task<ValueModel> ExecuteCommandAsync(CreateValueCommand command,
        CancellationToken cancellationToken)
    {
        var property = await propertyRepository.GetPropertyByIdAsync(command.Model.PropertyId, cancellationToken);
        if (property == null)
        {
            throw new Exception($"Property with ID {command.Model.PropertyId} not found");
        }

        var value = mapper.Map<Value>(command.Model);

        await valueRepository.AddValueAsync(value, cancellationToken);
        
        // Link value to property
        property.Values.Add(value);
        
        await valueRepository.SaveChangesAsync(cancellationToken);

        return mapper.Map<ValueModel>(value);
    }
}

public record CreateValueCommand(CreateValueModel Model);