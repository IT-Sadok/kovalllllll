using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Commands.PropertyCommands;

public class UpdatePropertyCommandHandler(IPropertyRepository propertyRepository, IMapper mapper)
    : ICommandHandler<UpdatePropertyCommand, PropertyResponseModel>
{
    public async Task<PropertyResponseModel> ExecuteCommandAsync(UpdatePropertyCommand command,
        CancellationToken cancellationToken)
    {
        var property = await propertyRepository.GetPropertyByIdAsync(command.PropertyId, cancellationToken);

        if (property is null)
        {
            throw new Exception($"Property with id {command.PropertyId} not found.");
        }

        mapper.Map(command.Model, property);

        await propertyRepository.SaveChangesAsync(cancellationToken);

        return mapper.Map<PropertyResponseModel>(property);
    }
}

public record UpdatePropertyCommand(Guid PropertyId, UpdatePropertyModel Model);