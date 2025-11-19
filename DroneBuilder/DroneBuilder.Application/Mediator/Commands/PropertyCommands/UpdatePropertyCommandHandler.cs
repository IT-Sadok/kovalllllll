using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Commands.PropertyCommands;

public class UpdatePropertyCommandHandler(IPropertyRepository propertyRepository, IMapper mapper)
    : ICommandHandler<UpdatePropertyCommand, PropertyModel>
{
    public async Task<PropertyModel> ExecuteCommandAsync(UpdatePropertyCommand command,
        CancellationToken cancellationToken)
    {
        var property = await propertyRepository.GetPropertyByIdAsync(command.PropertyId, cancellationToken);

        if (property is null)
        {
            throw new NotFoundException($"Property with id {command.PropertyId} not found.");
        }

        if (command.Model.Name is not null)
            property.Name = command.Model.Name;

        await propertyRepository.SaveChangesAsync(cancellationToken);

        return mapper.Map<PropertyModel>(property);
    }
}

public record UpdatePropertyCommand(Guid PropertyId, UpdatePropertyModel Model);