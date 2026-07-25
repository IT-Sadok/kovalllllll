using DroneBuilder.Application.Features.Catalog.Imports.Models;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Imports.ReplaceValueAliases;

public sealed class ReplaceValueAliasesCommandHandler(ICatalogImportRepository repository)
    : ICommandHandler<ReplaceValueAliasesCommand, ICollection<string>>
{
    public async Task<Result<ICollection<string>>> ExecuteCommandAsync(
        ReplaceValueAliasesCommand command,
        CancellationToken cancellationToken)
    {
        if (await repository.GetSourceAsync(command.SourceId, cancellationToken) is null)
        {
            return Result.Fail<ICollection<string>>(new NotFoundError(
                $"Import source with id {command.SourceId} not found."));
        }

        Value? value = await repository.GetValueAsync(command.ValueId, cancellationToken);
        if (value is null)
        {
            return Result.Fail<ICollection<string>>(new NotFoundError(
                $"Value with id {command.ValueId} not found."));
        }

        foreach (ValueAlias alias in value.Aliases
                     .Where(alias => alias.ImportSourceId == command.SourceId)
                     .ToList())
        {
            value.Aliases.Remove(alias);
        }

        List<string> aliases = command.Model.Aliases
            .Select(alias => alias.Trim())
            .DistinctBy(SpecificationAliasNormalizer.Normalize)
            .ToList();
        foreach (string alias in aliases)
        {
            value.Aliases.Add(new ValueAlias
            {
                ValueId = value.Id,
                ImportSourceId = command.SourceId,
                Alias = alias
            });
        }

        await repository.SaveChangesAsync(cancellationToken);
        return Result.Ok<ICollection<string>>(aliases);
    }
}

public sealed record ReplaceValueAliasesCommand(
    Guid SourceId,
    Guid ValueId,
    ReplaceSourceAliasesModel Model);
