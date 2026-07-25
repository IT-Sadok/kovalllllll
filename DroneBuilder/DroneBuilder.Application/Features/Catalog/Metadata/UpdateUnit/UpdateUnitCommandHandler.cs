using DroneBuilder.Application.Common;
using DroneBuilder.Application.Features.Catalog.Metadata.Models;
using DroneBuilder.Application.Features.Catalog.ProductSpecifications.Models;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Metadata.UpdateUnit;

public sealed class UpdateUnitCommandHandler(ICatalogMetadataRepository repository)
    : ICommandHandler<UpdateUnitCommand, UnitDefinitionModel>
{
    public async Task<Result<UnitDefinitionModel>> ExecuteCommandAsync(
        UpdateUnitCommand command,
        CancellationToken cancellationToken)
    {
        UnitDefinition? unit = await repository.GetUnitAsync(command.UnitId, cancellationToken);
        if (unit is null)
        {
            return Result.Fail<UnitDefinitionModel>(new NotFoundError(
                $"Unit with id {command.UnitId} not found."));
        }

        string targetCode = command.Model.Code is null ? unit.Code : EntityCode.FromName(command.Model.Code);
        if (await repository.IsUnitCodeInUseAsync(targetCode, unit.Id, cancellationToken))
        {
            return Result.Fail<UnitDefinitionModel>(new ConflictError($"Unit code '{targetCode}' already exists."));
        }

        string? targetDimension = command.Model.ClearDimension
            ? null
            : command.Model.Dimension is null
                ? unit.Dimension
                : EntityCode.FromName(command.Model.Dimension);
        decimal targetFactor = command.Model.ConversionFactorToBase ?? unit.ConversionFactorToBase;
        if (unit.Properties.Count > 0 &&
            (!string.Equals(targetDimension, unit.Dimension, StringComparison.Ordinal) ||
             targetFactor != unit.ConversionFactorToBase))
        {
            return Result.Fail<UnitDefinitionModel>(new ConflictError(
                "Dimension and conversion factor cannot be changed while properties use this unit."));
        }

        unit.Code = targetCode;
        unit.Name = command.Model.Name?.Trim() ?? unit.Name;
        unit.Symbol = command.Model.Symbol?.Trim() ?? unit.Symbol;
        unit.Dimension = targetDimension;
        unit.ConversionFactorToBase = targetFactor;
        unit.UpdatedAt = DateTime.UtcNow;

        if (command.Model.Aliases is not null)
        {
            unit.Aliases.Clear();
            foreach (string alias in command.Model.Aliases.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                unit.Aliases.Add(new UnitAlias { UnitDefinitionId = unit.Id, Alias = alias.Trim() });
            }
        }

        await repository.SaveChangesAsync(cancellationToken);
        return Result.Ok(unit.ToModel());
    }
}

public sealed record UpdateUnitCommand(Guid UnitId, UpdateUnitDefinitionModel Model);
