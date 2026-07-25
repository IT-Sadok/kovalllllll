using System.ComponentModel.DataAnnotations.Schema;

namespace DroneBuilder.Domain.Entities;

public class Product : AuditableEntity
{
    private decimal _pendingPrice;
    private string? _pendingCategoryName;

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid ProductCategoryId { get; set; }
    public ProductCategory? ProductCategory { get; set; }
    public ProductKind Kind { get; set; } = ProductKind.Other;
    public Guid? ComponentTypeId { get; set; }
    public ComponentType? ComponentType { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Image> Images { get; set; } = [];
    public ICollection<ProductVariant> Variants { get; set; } = [];
    public ICollection<ProductPropertyValue> ProductPropertyValues { get; set; } = [];
    public ICollection<ProductExternalReference> ExternalReferences { get; set; } = [];

    [NotMapped]
    public decimal Price
    {
        get => Variants.FirstOrDefault(v => v.IsDefault)?.Price ?? _pendingPrice;
        set
        {
            _pendingPrice = value;
            ProductVariant? defaultVariant = Variants.FirstOrDefault(v => v.IsDefault);
            if (defaultVariant is not null)
            {
                defaultVariant.Price = value;
                defaultVariant.UpdatedAt = DateTime.UtcNow;
            }
        }
    }

    [NotMapped]
    public string Category
    {
        get => _pendingCategoryName ?? ProductCategory?.Name ?? string.Empty;
        set => _pendingCategoryName = value;
    }

    public ProductVariant EnsureDefaultVariant(string? sku = null)
    {
        ProductVariant? defaultVariant = Variants.FirstOrDefault(v => v.IsDefault);
        if (defaultVariant is not null)
        {
            return defaultVariant;
        }

        defaultVariant = new ProductVariant
        {
            ProductId = Id,
            Product = this,
            Sku = sku ?? $"DB-{Id:N}",
            Price = _pendingPrice,
            IsDefault = true
        };

        Variants.Add(defaultVariant);
        return defaultVariant;
    }

    public void AssignCategory(ProductCategory category)
    {
        ProductCategory = category;
        ProductCategoryId = category.Id;
        _pendingCategoryName = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddSpecification(ProductPropertyValue specification)
    {
        ArgumentNullException.ThrowIfNull(specification);
        specification.Product = this;
        specification.ProductId = Id;
        specification.Validate();
        ComponentType?.RequirePropertyRule(specification.PropertyId, variantSpecific: false);

        if (!specification.Property!.AllowsMultipleValues &&
            ProductPropertyValues.Any(item => item.PropertyId == specification.PropertyId))
        {
            throw new InvalidOperationException(
                $"Property '{specification.Property.Code}' allows only one value per product.");
        }

        ProductPropertyValues.Add(specification);
    }

    public void ValidateForPublication()
    {
        if (!IsActive)
        {
            return;
        }

        int defaultVariantCount = Variants.Count(variant => variant is
        {
            IsActive: true,
            IsDefault: true
        });

        if (defaultVariantCount != 1)
        {
            throw new InvalidOperationException(
                $"Active product '{Id}' must have exactly one active default variant.");
        }

        if (ComponentTypeId.HasValue && ComponentType is null)
        {
            throw new InvalidOperationException(
                $"Component type metadata must be loaded before publishing product '{Id}'.");
        }

        if (ComponentType is null)
        {
            return;
        }

        foreach (ComponentTypeProperty rule in ComponentType.Properties.Where(rule => rule.IsRequired))
        {
            bool hasRequiredValue = rule.IsVariantSpecific
                ? Variants
                    .Where(variant => variant.IsActive)
                    .All(variant => variant.Specifications.Any(
                        value => value.PropertyId == rule.PropertyId))
                : ProductPropertyValues.Any(value => value.PropertyId == rule.PropertyId);

            if (!hasRequiredValue)
            {
                throw new InvalidOperationException(
                    $"Required property '{rule.PropertyId}' is missing for component type '{ComponentType.Code}'.");
            }
        }
    }
}
