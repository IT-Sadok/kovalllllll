namespace DroneBuilder.Domain.Entities;

public class Property : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public SpecificationDataType DataType { get; set; } = SpecificationDataType.Option;
    public Guid? UnitDefinitionId { get; set; }
    public UnitDefinition? UnitDefinition { get; set; }
    public bool IsFilterable { get; set; } = true;
    public bool IsCompatibilityRelevant { get; set; }
    public bool AllowsMultipleValues { get; set; } = true;
    public ICollection<Value> Values { get; set; } = [];
    public ICollection<ProductPropertyValue> ProductPropertyValues { get; set; } = [];
    public ICollection<ProductVariantPropertyValue> ProductVariantPropertyValues { get; set; } = [];
    public ICollection<ComponentTypeProperty> ComponentTypes { get; set; } = [];
    public ICollection<PropertyAlias> Aliases { get; set; } = [];

    public void ValidateDefinition()
    {
        bool numericProperty = DataType is SpecificationDataType.Number
            or SpecificationDataType.NumericRange;

        if (!numericProperty && UnitDefinitionId.HasValue)
        {
            throw new InvalidOperationException(
                $"Non-numeric property '{Code}' cannot have a measurement unit.");
        }

        if (numericProperty && IsCompatibilityRelevant && !UnitDefinitionId.HasValue)
        {
            throw new InvalidOperationException(
                $"Compatibility property '{Code}' requires a canonical measurement unit.");
        }
    }
}
