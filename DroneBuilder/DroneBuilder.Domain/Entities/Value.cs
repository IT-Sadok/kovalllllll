namespace DroneBuilder.Domain.Entities;

public class Value : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public decimal? NumericValue { get; set; }
    public bool? BooleanValue { get; set; }
    public ICollection<Property> Properties { get; set; } = [];
    public ICollection<ProductPropertyValue> ProductPropertyValues { get; set; } = [];
    public ICollection<ProductVariantPropertyValue> ProductVariantPropertyValues { get; set; } = [];
    public ICollection<ValueAlias> Aliases { get; set; } = [];
}
