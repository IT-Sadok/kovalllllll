using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Commands.PropertyCommands;

public class UpdatePropertyCommandHandler(IPropertyRepository propertyRepository, IMapper mapper)
    : ICommandHandler<UpdatePropertyCommand, PropertyModel>
{
    public async Task<Result<PropertyModel>> ExecuteCommandAsync(UpdatePropertyCommand command,
        CancellationToken cancellationToken)
    {
        Property? property = await propertyRepository.GetPropertyByIdAsync(command.PropertyId, cancellationToken);

        if (property is null)
        {
            return Result.Fail<PropertyModel>(new NotFoundError($"Property with id {command.PropertyId} not found."));
        }

        if (command.Model.Name is not null)
        {
            property.Name = command.Model.Name;
        }

        await propertyRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok(mapper.Map<PropertyModel>(property));
    }
}

public record UpdatePropertyCommand(Guid PropertyId, UpdatePropertyModel Model);
