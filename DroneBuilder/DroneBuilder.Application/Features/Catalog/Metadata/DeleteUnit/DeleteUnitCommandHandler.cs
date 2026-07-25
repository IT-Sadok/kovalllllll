using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Metadata.DeleteUnit;

public sealed class DeleteUnitCommandHandler(ICatalogMetadataRepository repository)
    : ICommandHandler<DeleteUnitCommand>
{
    public async Task<Result> ExecuteCommandAsync(DeleteUnitCommand command, CancellationToken cancellationToken)
    {
        UnitDefinition? unit = await repository.GetUnitAsync(command.UnitId, cancellationToken);
        if (unit is null)
        {
            return Result.Fail(new NotFoundError($"Unit with id {command.UnitId} not found."));
        }

        if (unit.Properties.Count > 0)
        {
            return Result.Fail(new ConflictError($"Unit '{unit.Code}' is in use and cannot be deleted."));
        }

        repository.RemoveUnit(unit);
        await repository.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}

public sealed record DeleteUnitCommand(Guid UnitId);
