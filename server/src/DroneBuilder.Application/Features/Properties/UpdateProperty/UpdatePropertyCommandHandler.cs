using DroneBuilder.Application.Common.Mediator.Interfaces;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Application.Common.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;
namespace DroneBuilder.Application.Features.Properties.UpdateProperty;

public class UpdatePropertyCommandHandler(IPropertyRepository propertyRepository)
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

        command.Model.UpdateEntity(property);

        await propertyRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok(property.ToModel());
    }
}

public record UpdatePropertyCommand(Guid PropertyId, UpdatePropertyModel Model);
