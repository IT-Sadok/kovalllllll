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

    public void UpdateDetails(string? sku, string? name, decimal? price, string? currencyCode)
    {
        if (sku is not null)
        {
            Sku = sku.Trim();
        }

        if (name is not null)
        {
            Name = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
        }

        if (price.HasValue)
        {
            if (price.Value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(price));
            }

            Price = price.Value;
        }

        if (currencyCode is not null)
        {
            if (currencyCode.Trim().Length != 3)
            {
                throw new ArgumentException("Currency code must contain exactly three characters.", nameof(currencyCode));
            }

            CurrencyCode = currencyCode.Trim().ToUpperInvariant();
        }

        UpdatedAt = DateTime.UtcNow;
        Product?.BeginDraft();
    }
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
        Product?.BeginDraft();
    }
    public void RemoveSpecification(ProductVariantPropertyValue specification)
    {
        ArgumentNullException.ThrowIfNull(specification);

        ComponentTypeProperty? rule = Product?.ComponentType?.Properties.FirstOrDefault(
            item => item.PropertyId == specification.PropertyId);
        bool hasAnotherValue = Specifications.Any(
            item => item.Id != specification.Id && item.PropertyId == specification.PropertyId);
        if (rule is { IsRequired: true } && !hasAnotherValue)
        {
            throw new InvalidOperationException(
                $"Required property '{specification.PropertyId}' cannot be removed from variant '{Id}'.");
        }

        Specifications.Remove(specification);
        Product?.BeginDraft();
    }
}
