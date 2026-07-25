namespace DroneBuilder.Domain.Entities;

public class ProductPropertyValue : AuditableEntity
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    public Guid PropertyId { get; set; }
    public Property? Property { get; set; }

    public Guid? ValueId { get; set; }
    public Value? Value { get; set; }
    public string? TextValue { get; set; }
    public decimal? NumericValue { get; set; }
    public decimal? MinNumericValue { get; set; }
    public decimal? MaxNumericValue { get; set; }
    public bool? BooleanValue { get; set; }

    public void SetValue(
        Guid? valueId,
        Value? value,
        string? textValue,
        decimal? numericValue,
        decimal? minNumericValue,
        decimal? maxNumericValue,
        bool? booleanValue)
    {
        SpecificationValueRules.Validate(
            Property ?? throw new InvalidOperationException("Property metadata is required."),
            valueId,
            textValue,
            numericValue,
            minNumericValue,
            maxNumericValue,
            booleanValue);

        ValueId = valueId;
        Value = value;
        TextValue = textValue;
        NumericValue = numericValue;
        MinNumericValue = minNumericValue;
        MaxNumericValue = maxNumericValue;
        BooleanValue = booleanValue;
        UpdatedAt = DateTime.UtcNow;
        Product?.BeginDraft();
    }
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
