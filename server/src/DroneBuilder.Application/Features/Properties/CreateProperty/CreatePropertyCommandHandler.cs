using DroneBuilder.Application.Common.Mediator.Interfaces;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Domain.Entities;
using FluentResults;
namespace DroneBuilder.Application.Features.Properties.CreateProperty;

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
