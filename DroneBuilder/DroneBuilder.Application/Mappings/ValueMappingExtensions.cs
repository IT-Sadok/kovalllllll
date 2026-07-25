using DroneBuilder.Application.Common;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Mappings;

public static class ValueMappingExtensions
{
    public static ValueModel ToModel(this Value value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return new ValueModel
        {
            Id = value.Id,
            Code = value.Code,
            Text = value.Text,
            NumericValue = value.NumericValue,
            BooleanValue = value.BooleanValue,
            Aliases = value.Aliases.OrderBy(alias => alias.Alias).Select(alias => alias.Alias).ToList()
        };
    }

    public static Value ToEntity(this CreateValueModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var value = new Value
        {
            Code = string.IsNullOrWhiteSpace(model.Code) ? EntityCode.FromName(model.Text) : model.Code.Trim(),
            Text = model.Text.Trim(),
            NumericValue = model.NumericValue,
            BooleanValue = model.BooleanValue
        };

        foreach (string alias in model.Aliases.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            value.Aliases.Add(new ValueAlias { Alias = alias.Trim() });
        }

        return value;
    }
}
