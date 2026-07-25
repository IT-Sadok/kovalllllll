using DroneBuilder.Application.Common;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Mappings;

public static class PropertyMappingExtensions
{
    public static PropertyModel ToModel(this Property property)
    {
        ArgumentNullException.ThrowIfNull(property);

        return new PropertyModel
        {
            Id = property.Id,
            Code = property.Code,
            Name = property.Name,
            DataType = property.DataType.ToString(),
            UnitDefinitionId = property.UnitDefinitionId,
            IsFilterable = property.IsFilterable,
            IsCompatibilityRelevant = property.IsCompatibilityRelevant,
            AllowsMultipleValues = property.AllowsMultipleValues,
            Aliases = property.Aliases.OrderBy(alias => alias.Alias).Select(alias => alias.Alias).ToList(),
            Values = property.Values.Select(value => value.ToModel()).ToList()
        };
    }

    public static Property ToEntity(this CreatePropertyModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var property = new Property
        {
            Code = string.IsNullOrWhiteSpace(model.Code) ? EntityCode.FromName(model.Name) : model.Code.Trim(),
            Name = model.Name.Trim(),
            DataType = Enum.Parse<SpecificationDataType>(model.DataType, ignoreCase: true),
            UnitDefinitionId = model.UnitDefinitionId,
            IsFilterable = model.IsFilterable,
            IsCompatibilityRelevant = model.IsCompatibilityRelevant,
            AllowsMultipleValues = model.AllowsMultipleValues,
            Values = model.Values.Select(value => value.ToEntity()).ToList()
        };

        foreach (string alias in model.Aliases.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            property.Aliases.Add(new PropertyAlias { Alias = alias.Trim() });
        }

        return property;
    }
}
