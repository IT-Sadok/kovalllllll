namespace DroneBuilder.Domain.Entities;

public class ProductVariant : AuditableEntity
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string? Name { get; set; }
    public decimal Price { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<ProductVariantPropertyValue> Specifications { get; set; } = [];
    public ICollection<WarehouseItem> WarehouseItems { get; set; } = [];
    public ICollection<CartItem> CartItems { get; set; } = [];
    public ICollection<OrderItem> OrderItems { get; set; } = [];
    public ICollection<ProductVariantExternalReference> ExternalReferences { get; set; } = [];

    public void AddSpecification(ProductVariantPropertyValue specification)
    {
        ArgumentNullException.ThrowIfNull(specification);
        specification.ProductVariant = this;
        specification.ProductVariantId = Id;
        specification.Validate();
        Product?.ComponentType?.RequirePropertyRule(
            specification.PropertyId,
            variantSpecific: true);

        if (!specification.Property!.AllowsMultipleValues &&
            Specifications.Any(item => item.PropertyId == specification.PropertyId))
        {
            throw new InvalidOperationException(
                $"Property '{specification.Property.Code}' allows only one value per variant.");
        }

        Specifications.Add(specification);
    }
}
