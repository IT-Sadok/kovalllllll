using DroneBuilder.Application.Mappings;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using FluentResults;
namespace DroneBuilder.Application.Mediator.Commands.PropertyCommands;

public class CreatePropertyCommandHandler(IPropertyRepository propertyRepository)
    : ICommandHandler<CreatePropertyCommand, PropertyModel>
{
    public async Task<Result<PropertyModel>> ExecuteCommandAsync(CreatePropertyCommand command,
        CancellationToken cancellationToken)
    {
        Property property = command.Model.ToEntity();

        await propertyRepository.AddPropertyAsync(property, cancellationToken);
        await propertyRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok(property.ToModel());
    }
}

public record CreatePropertyCommand(CreatePropertyModel Model);
