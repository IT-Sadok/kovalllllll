using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Metadata.DeleteComponentType;

public sealed class DeleteComponentTypeCommandHandler(ICatalogMetadataRepository repository)
    : ICommandHandler<DeleteComponentTypeCommand>
{
    public async Task<Result> ExecuteCommandAsync(
        DeleteComponentTypeCommand command,
        CancellationToken cancellationToken)
    {
        ComponentType? componentType = await repository.GetComponentTypeAsync(
            command.ComponentTypeId,
            cancellationToken);
        if (componentType is null)
        {
            return Result.Fail(new NotFoundError(
                $"Component type with id {command.ComponentTypeId} not found."));
        }

        if (componentType.Products.Any(product => product.IsActive))
        {
            return Result.Fail(new ConflictError(
                "A component type used by active products cannot be deactivated."));
        }

        componentType.IsActive = false;
        componentType.UpdatedAt = DateTime.UtcNow;
        await repository.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}

public sealed record DeleteComponentTypeCommand(Guid ComponentTypeId);
