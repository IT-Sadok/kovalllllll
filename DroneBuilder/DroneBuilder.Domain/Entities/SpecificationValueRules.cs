namespace DroneBuilder.Domain.Entities;

public static class SpecificationValueRules
{
    public static void Validate(
        Property property,
        Guid? valueId,
        string? textValue,
        decimal? numericValue,
        decimal? minNumericValue,
        decimal? maxNumericValue,
        bool? booleanValue)
    {
        ArgumentNullException.ThrowIfNull(property);

        bool hasOption = valueId.HasValue;
        bool hasText = textValue is not null;
        bool hasNumber = numericValue.HasValue;
        bool hasRange = minNumericValue.HasValue || maxNumericValue.HasValue;
        bool hasBoolean = booleanValue.HasValue;

        int populatedRepresentations =
            Convert.ToInt32(hasOption) +
            Convert.ToInt32(hasText) +
            Convert.ToInt32(hasNumber) +
            Convert.ToInt32(hasRange) +
            Convert.ToInt32(hasBoolean);

        if (populatedRepresentations != 1)
        {
            throw new InvalidOperationException(
                $"Property '{property.Code}' must have exactly one value representation.");
        }

        if (hasRange && (!minNumericValue.HasValue || !maxNumericValue.HasValue))
        {
            throw new InvalidOperationException(
                $"Range property '{property.Code}' requires both minimum and maximum values.");
        }

        if (minNumericValue > maxNumericValue)
        {
            throw new InvalidOperationException(
                $"Range property '{property.Code}' cannot have a minimum greater than its maximum.");
        }

        bool validForDataType = property.DataType switch
        {
            SpecificationDataType.Text => hasText,
            SpecificationDataType.Number => hasNumber,
            SpecificationDataType.Boolean => hasBoolean,
            SpecificationDataType.Option => hasOption,
            SpecificationDataType.NumericRange => hasRange,
            _ => false
        };

        if (!validForDataType)
        {
            throw new InvalidOperationException(
                $"Value representation does not match data type '{property.DataType}' for property '{property.Code}'.");
        }

        bool numericProperty = property.DataType is SpecificationDataType.Number
            or SpecificationDataType.NumericRange;

        if (!numericProperty && property.UnitDefinitionId.HasValue)
        {
            throw new InvalidOperationException(
                $"Non-numeric property '{property.Code}' cannot have a measurement unit.");
        }

        if (numericProperty &&
            property.IsCompatibilityRelevant &&
            !property.UnitDefinitionId.HasValue)
        {
            throw new InvalidOperationException(
                $"Compatibility property '{property.Code}' requires a canonical measurement unit.");
        }
    }
}
