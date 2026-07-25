using DroneBuilder.Application.Common;
using DroneBuilder.Application.Mappings;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Properties.UpdateProperty;

public class UpdatePropertyCommandHandler(IPropertyRepository propertyRepository)
    : ICommandHandler<UpdatePropertyCommand, PropertyModel>
{
    public async Task<Result<PropertyModel>> ExecuteCommandAsync(
        UpdatePropertyCommand command,
        CancellationToken cancellationToken)
    {
        Property? property = await propertyRepository.GetPropertyByIdAsync(command.PropertyId, cancellationToken);
        if (property is null)
        {
            return Result.Fail<PropertyModel>(new NotFoundError($"Property with id {command.PropertyId} not found."));
        }

        SpecificationDataType targetDataType = property.DataType;
        if (command.Model.DataType is not null &&
            !Enum.TryParse(command.Model.DataType, ignoreCase: true, out targetDataType))
        {
            return Result.Fail<PropertyModel>(new ValidationError(
                $"Unsupported property data type '{command.Model.DataType}'."));
        }

        string targetCode = command.Model.Code is null
            ? property.Code
            : EntityCode.FromName(command.Model.Code);
        if (await propertyRepository.IsCodeInUseAsync(targetCode, property.Id, cancellationToken))
        {
            return Result.Fail<PropertyModel>(new ConflictError($"Property code '{targetCode}' already exists."));
        }

        Guid? targetUnitId = command.Model.ClearUnitDefinition
            ? null
            : command.Model.UnitDefinitionId ?? property.UnitDefinitionId;
        if (targetUnitId.HasValue &&
            await propertyRepository.GetUnitDefinitionAsync(targetUnitId.Value, cancellationToken) is null)
        {
            return Result.Fail<PropertyModel>(new NotFoundError($"Unit with id {targetUnitId.Value} not found."));
        }

        if (targetDataType != SpecificationDataType.Option && property.Values.Count > 0)
        {
            return Result.Fail<PropertyModel>(new ConflictError(
                "Remove predefined values before changing an Option property to another data type."));
        }

        string oldCode = property.Code;
        string oldName = property.Name;
        SpecificationDataType oldDataType = property.DataType;
        Guid? oldUnitId = property.UnitDefinitionId;
        bool oldIsFilterable = property.IsFilterable;
        bool oldIsCompatibilityRelevant = property.IsCompatibilityRelevant;
        bool oldAllowsMultipleValues = property.AllowsMultipleValues;

        property.Code = targetCode;
        property.Name = command.Model.Name?.Trim() ?? property.Name;
        property.DataType = targetDataType;
        property.UnitDefinitionId = targetUnitId;
        property.IsFilterable = command.Model.IsFilterable ?? property.IsFilterable;
        property.IsCompatibilityRelevant = command.Model.IsCompatibilityRelevant ?? property.IsCompatibilityRelevant;
        property.AllowsMultipleValues = command.Model.AllowsMultipleValues ?? property.AllowsMultipleValues;

        try
        {
            property.ValidateDefinition();
            foreach (ProductPropertyValue specification in property.ProductPropertyValues)
            {
                specification.Validate();
            }

            foreach (ProductVariantPropertyValue specification in property.ProductVariantPropertyValues)
            {
                specification.Validate();
            }
        }
        catch (InvalidOperationException exception)
        {
            property.Code = oldCode;
            property.Name = oldName;
            property.DataType = oldDataType;
            property.UnitDefinitionId = oldUnitId;
            property.IsFilterable = oldIsFilterable;
            property.IsCompatibilityRelevant = oldIsCompatibilityRelevant;
            property.AllowsMultipleValues = oldAllowsMultipleValues;
            return Result.Fail<PropertyModel>(new ConflictError(exception.Message));
        }

        if (!property.AllowsMultipleValues &&
            (property.ProductPropertyValues.GroupBy(value => value.ProductId).Any(group => group.Count() > 1) ||
             property.ProductVariantPropertyValues.GroupBy(value => value.ProductVariantId).Any(group => group.Count() > 1)))
        {
            property.AllowsMultipleValues = oldAllowsMultipleValues;
            return Result.Fail<PropertyModel>(new ConflictError(
                "Existing products contain multiple values for this property."));
        }

        if (command.Model.Aliases is not null)
        {
            foreach (PropertyAlias alias in property.Aliases.Where(alias => alias.ImportSourceId is null).ToList())
            {
                property.Aliases.Remove(alias);
            }

            foreach (string alias in command.Model.Aliases.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                property.Aliases.Add(new PropertyAlias { PropertyId = property.Id, Alias = alias.Trim() });
            }
        }

        await propertyRepository.SaveChangesAsync(cancellationToken);
        return Result.Ok(property.ToModel());
    }
}

public sealed record UpdatePropertyCommand(Guid PropertyId, UpdatePropertyModel Model);
