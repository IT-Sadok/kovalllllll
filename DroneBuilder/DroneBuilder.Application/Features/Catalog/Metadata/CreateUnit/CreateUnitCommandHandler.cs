using DroneBuilder.Application.Common;
using DroneBuilder.Application.Features.Catalog.Metadata.Models;
using DroneBuilder.Application.Features.Catalog.ProductSpecifications.Models;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Metadata.CreateUnit;

public sealed class CreateUnitCommandHandler(ICatalogMetadataRepository repository)
    : ICommandHandler<CreateUnitCommand, UnitDefinitionModel>
{
    public async Task<Result<UnitDefinitionModel>> ExecuteCommandAsync(
        CreateUnitCommand command,
        CancellationToken cancellationToken)
    {
        string code = EntityCode.FromName(command.Model.Code);
        if (await repository.IsUnitCodeInUseAsync(code, cancellationToken: cancellationToken))
        {
            return Result.Fail<UnitDefinitionModel>(new ConflictError($"Unit code '{code}' already exists."));
        }

        var unit = new UnitDefinition
        {
            Code = code,
            Name = command.Model.Name.Trim(),
            Symbol = command.Model.Symbol.Trim(),
            Dimension = string.IsNullOrWhiteSpace(command.Model.Dimension)
                ? null
                : EntityCode.FromName(command.Model.Dimension),
            ConversionFactorToBase = command.Model.ConversionFactorToBase
        };
        foreach (string alias in command.Model.Aliases.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            unit.Aliases.Add(new UnitAlias { Alias = alias.Trim() });
        }

        await repository.AddUnitAsync(unit, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return Result.Ok(unit.ToModel());
    }
}

public sealed record CreateUnitCommand(CreateUnitDefinitionModel Model);
