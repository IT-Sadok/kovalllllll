using System.ComponentModel.DataAnnotations.Schema;

namespace DroneBuilder.Domain.Entities;

public class CartItem : AuditableEntity
{
    private Guid _legacyProductId;
    private Product? _legacyProduct;
    private string _legacyProductName = string.Empty;

    public int Quantity { get; set; }
    public Guid ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }
    public Guid CartId { get; set; }
    public Cart? Cart { get; set; }
    public InventoryReservation? Reservation { get; set; }

    [NotMapped]
    public Guid ProductId
    {
        get => ProductVariant?.ProductId ?? _legacyProductId;
        set => _legacyProductId = value;
    }

    [NotMapped]
    public Product? Product
    {
        get => ProductVariant?.Product ?? _legacyProduct;
        set => _legacyProduct = value;
    }

    [NotMapped]
    public string ProductName
    {
        get => Product?.Name ?? _legacyProductName;
        set => _legacyProductName = value;
    }

    public void AttachVariant(ProductVariant variant)
    {
        ProductVariant = variant;
        ProductVariantId = variant.Id;
        _legacyProductId = variant.ProductId;
        _legacyProduct = variant.Product;
    }
}
