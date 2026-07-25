using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Imports.DeleteImportSource;

public sealed class DeleteImportSourceCommandHandler(ICatalogImportRepository repository)
    : ICommandHandler<DeleteImportSourceCommand>
{
    public async Task<Result> ExecuteCommandAsync(
        DeleteImportSourceCommand command,
        CancellationToken cancellationToken)
    {
        ImportSource? source = await repository.GetSourceAsync(command.SourceId, cancellationToken);
        if (source is null)
        {
            return Result.Fail(new NotFoundError($"Import source with id {command.SourceId} not found."));
        }

        source.IsActive = false;
        source.UpdatedAt = DateTime.UtcNow;
        await repository.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}

public sealed record DeleteImportSourceCommand(Guid SourceId);
