using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Metadata.DeleteComponentTypeProperty;

public sealed class DeleteComponentTypePropertyCommandHandler(ICatalogMetadataRepository repository)
    : ICommandHandler<DeleteComponentTypePropertyCommand>
{
    public async Task<Result> ExecuteCommandAsync(
        DeleteComponentTypePropertyCommand command,
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

        ComponentTypeProperty? rule = componentType.Properties.FirstOrDefault(
            item => item.PropertyId == command.PropertyId);
        if (rule is null)
        {
            return Result.Fail(new NotFoundError(
                $"Property rule for property {command.PropertyId} was not found."));
        }

        try
        {
            componentType.RemovePropertyRule(rule);
        }
        catch (InvalidOperationException exception)
        {
            return Result.Fail(new ConflictError(exception.Message));
        }

        repository.RemoveComponentTypeRule(rule);
        await repository.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}

public sealed record DeleteComponentTypePropertyCommand(Guid ComponentTypeId, Guid PropertyId);
