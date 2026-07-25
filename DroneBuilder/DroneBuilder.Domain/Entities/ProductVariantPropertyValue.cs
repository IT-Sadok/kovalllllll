namespace DroneBuilder.Domain.Entities;

public class ProductVariantPropertyValue : AuditableEntity
{
    public Guid ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }
    public Guid PropertyId { get; set; }
    public Property? Property { get; set; }
    public Guid? ValueId { get; set; }
    public Value? Value { get; set; }
    public string? TextValue { get; set; }
    public decimal? NumericValue { get; set; }
    public decimal? MinNumericValue { get; set; }
    public decimal? MaxNumericValue { get; set; }
    public bool? BooleanValue { get; set; }

    public void Validate()
    {
        SpecificationValueRules.Validate(
            Property ?? throw new InvalidOperationException("Property metadata is required."),
            ValueId,
            TextValue,
            NumericValue,
            MinNumericValue,
            MaxNumericValue,
            BooleanValue);
    }
}
