using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Properties.DeleteProperty;

public class DeletePropertyCommandHandler(IPropertyRepository propertyRepository)
    : ICommandHandler<DeletePropertyCommand>
{
    public async Task<Result> ExecuteCommandAsync(DeletePropertyCommand command, CancellationToken cancellationToken)
    {
        Property? property = await propertyRepository.GetPropertyByIdAsync(command.PropertyId, cancellationToken);
        if (property is null)
        {
            return Result.Fail(new NotFoundError($"Property with id {command.PropertyId} not found."));
        }

        if (property.ComponentTypes.Count > 0 ||
            property.ProductPropertyValues.Count > 0 ||
            property.ProductVariantPropertyValues.Count > 0)
        {
            return Result.Fail(new ConflictError(
                $"Property '{property.Code}' is in use and cannot be deleted."));
        }

        propertyRepository.RemoveProperty(property);
        await propertyRepository.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}

public sealed record DeletePropertyCommand(Guid PropertyId);
