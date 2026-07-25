using DroneBuilder.Application.Features.Catalog.Imports.Models;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Imports.ReplacePropertyAliases;

public sealed class ReplacePropertyAliasesCommandHandler(ICatalogImportRepository repository)
    : ICommandHandler<ReplacePropertyAliasesCommand, ICollection<string>>
{
    public async Task<Result<ICollection<string>>> ExecuteCommandAsync(
        ReplacePropertyAliasesCommand command,
        CancellationToken cancellationToken)
    {
        if (await repository.GetSourceAsync(command.SourceId, cancellationToken) is null)
        {
            return Result.Fail<ICollection<string>>(new NotFoundError(
                $"Import source with id {command.SourceId} not found."));
        }

        Property? property = await repository.GetPropertyAsync(command.PropertyId, cancellationToken);
        if (property is null)
        {
            return Result.Fail<ICollection<string>>(new NotFoundError(
                $"Property with id {command.PropertyId} not found."));
        }

        foreach (PropertyAlias alias in property.Aliases
                     .Where(alias => alias.ImportSourceId == command.SourceId)
                     .ToList())
        {
            property.Aliases.Remove(alias);
        }

        List<string> aliases = command.Model.Aliases
            .Select(alias => alias.Trim())
            .DistinctBy(SpecificationAliasNormalizer.Normalize)
            .ToList();
        foreach (string alias in aliases)
        {
            property.Aliases.Add(new PropertyAlias
            {
                PropertyId = property.Id,
                ImportSourceId = command.SourceId,
                Alias = alias
            });
        }

        await repository.SaveChangesAsync(cancellationToken);
        return Result.Ok<ICollection<string>>(aliases);
    }
}

public sealed record ReplacePropertyAliasesCommand(
    Guid SourceId,
    Guid PropertyId,
    ReplaceSourceAliasesModel Model);
