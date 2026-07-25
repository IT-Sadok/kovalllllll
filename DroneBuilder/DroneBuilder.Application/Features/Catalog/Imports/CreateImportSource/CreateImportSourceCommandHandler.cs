using DroneBuilder.Application.Common;
using DroneBuilder.Application.Features.Catalog.Imports.Models;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Imports.CreateImportSource;

public sealed class CreateImportSourceCommandHandler(ICatalogImportRepository repository)
    : ICommandHandler<CreateImportSourceCommand, ImportSourceModel>
{
    public async Task<Result<ImportSourceModel>> ExecuteCommandAsync(
        CreateImportSourceCommand command,
        CancellationToken cancellationToken)
    {
        string code = EntityCode.FromName(command.Model.Code);
        if (await repository.IsSourceCodeInUseAsync(code, cancellationToken: cancellationToken))
        {
            return Result.Fail<ImportSourceModel>(new ConflictError($"Import source code '{code}' already exists."));
        }

        var source = new ImportSource
        {
            Code = code,
            Name = command.Model.Name.Trim(),
            BaseUrl = string.IsNullOrWhiteSpace(command.Model.BaseUrl) ? null : command.Model.BaseUrl.Trim()
        };
        await repository.AddSourceAsync(source, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return Result.Ok(source.ToModel());
    }
}

public sealed record CreateImportSourceCommand(CreateImportSourceModel Model);
