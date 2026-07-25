using DroneBuilder.Application.Common;
using DroneBuilder.Application.Features.Catalog.Imports.Models;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Imports.UpdateImportSource;

public sealed class UpdateImportSourceCommandHandler(ICatalogImportRepository repository)
    : ICommandHandler<UpdateImportSourceCommand, ImportSourceModel>
{
    public async Task<Result<ImportSourceModel>> ExecuteCommandAsync(
        UpdateImportSourceCommand command,
        CancellationToken cancellationToken)
    {
        ImportSource? source = await repository.GetSourceAsync(command.SourceId, cancellationToken);
        if (source is null)
        {
            return Result.Fail<ImportSourceModel>(new NotFoundError($"Import source with id {command.SourceId} not found."));
        }

        string targetCode = command.Model.Code is null ? source.Code : EntityCode.FromName(command.Model.Code);
        if (await repository.IsSourceCodeInUseAsync(targetCode, source.Id, cancellationToken))
        {
            return Result.Fail<ImportSourceModel>(new ConflictError($"Import source code '{targetCode}' already exists."));
        }

        source.Code = targetCode;
        source.Name = command.Model.Name?.Trim() ?? source.Name;
        source.BaseUrl = command.Model.ClearBaseUrl
            ? null
            : command.Model.BaseUrl?.Trim() ?? source.BaseUrl;
        source.IsActive = command.Model.IsActive ?? source.IsActive;
        source.UpdatedAt = DateTime.UtcNow;
        await repository.SaveChangesAsync(cancellationToken);
        return Result.Ok(source.ToModel());
    }
}

public sealed record UpdateImportSourceCommand(Guid SourceId, UpdateImportSourceModel Model);
