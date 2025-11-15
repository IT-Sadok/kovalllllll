using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Commands.PropertyCommands;

public class CreatePropertyCommandHandler(IPropertyRepository propertyRepository, IMapper mapper)
    : ICommandHandler<CreatePropertyCommand, PropertyResponseModel>
{
    public async Task<PropertyResponseModel> ExecuteCommandAsync(CreatePropertyCommand command,
        CancellationToken cancellationToken)
    {
        var property = mapper.Map<Property>(command.Model);

        await propertyRepository.AddPropertyAsync(property, cancellationToken);
        await propertyRepository.SaveChangesAsync(cancellationToken);

        return mapper.Map<PropertyResponseModel>(property);
    }
}

public record CreatePropertyCommand(CreatePropertyModel Model);