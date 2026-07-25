using DroneBuilder.Application.Features.Catalog.Metadata.Models;
using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Features.Catalog.ProductSpecifications.Models;

public static class CatalogSpecificationMappings
{
    public static ComponentTypeModel ToModel(this ComponentType componentType)
    {
        return new ComponentTypeModel
        {
            Id = componentType.Id,
            Code = componentType.Code,
            Name = componentType.Name,
            IsActive = componentType.IsActive
        };
    }

    public static ComponentTypeDetailsModel ToDetailsModel(this ComponentType componentType)
    {
        return new ComponentTypeDetailsModel
        {
            Id = componentType.Id,
            Code = componentType.Code,
            Name = componentType.Name,
            IsActive = componentType.IsActive,
            Properties = componentType.Properties
                .OrderBy(rule => rule.SortOrder)
                .ThenBy(rule => rule.Property!.Name)
                .Select(rule => rule.ToModel())
                .ToList()
        };
    }

    public static ComponentTypePropertyModel ToModel(this ComponentTypeProperty rule)
    {
        Property property = rule.Property
            ?? throw new InvalidOperationException("Component type property metadata is not loaded.");

        return new ComponentTypePropertyModel
        {
            RuleId = rule.Id,
            PropertyId = property.Id,
            Code = property.Code,
            Name = property.Name,
            DataType = property.DataType.ToString(),
            IsRequired = rule.IsRequired,
            IsVariantSpecific = rule.IsVariantSpecific,
            AllowsMultipleValues = property.AllowsMultipleValues,
            IsFilterable = property.IsFilterable,
            IsCompatibilityRelevant = property.IsCompatibilityRelevant,
            SortOrder = rule.SortOrder,
            Unit = property.UnitDefinition?.ToModel(),
            Options = property.Values
                .OrderBy(value => value.Text)
                .Select(value => value.ToModel())
                .ToList()
        };
    }

    public static UnitDefinitionModel ToModel(this UnitDefinition unit)
    {
        return new UnitDefinitionModel
        {
            Id = unit.Id,
            Code = unit.Code,
            Name = unit.Name,
            Symbol = unit.Symbol,
            Dimension = unit.Dimension,
            ConversionFactorToBase = unit.ConversionFactorToBase,
            Aliases = unit.Aliases
                .OrderBy(alias => alias.Alias)
                .Select(alias => alias.Alias)
                .ToList()
        };
    }

    public static SpecificationOptionModel ToModel(this Value value)
    {
        return new SpecificationOptionModel
        {
            Id = value.Id,
            Code = value.Code,
            Text = value.Text,
            NumericValue = value.NumericValue,
            BooleanValue = value.BooleanValue
        };
    }

    public static ProductSpecificationModel ToModel(this ProductPropertyValue specification)
    {
        Property property = specification.Property
            ?? throw new InvalidOperationException("Specification property metadata is not loaded.");

        return new ProductSpecificationModel
        {
            Id = specification.Id,
            ProductId = specification.ProductId,
            PropertyId = property.Id,
            PropertyCode = property.Code,
            PropertyName = property.Name,
            DataType = property.DataType.ToString(),
            Unit = property.UnitDefinition?.ToModel(),
            ValueId = specification.ValueId,
            Option = specification.Value?.ToModel(),
            TextValue = specification.TextValue,
            NumericValue = specification.NumericValue,
            MinNumericValue = specification.MinNumericValue,
            MaxNumericValue = specification.MaxNumericValue,
            BooleanValue = specification.BooleanValue
        };
    }

    public static void SetValue(
        this ProductPropertyValue specification,
        SpecificationValueInputModel model,
        Value? option)
    {
        specification.SetValue(
            model.ValueId,
            option,
            model.TextValue,
            model.NumericValue,
            model.MinNumericValue,
            model.MaxNumericValue,
            model.BooleanValue);
    }
}
