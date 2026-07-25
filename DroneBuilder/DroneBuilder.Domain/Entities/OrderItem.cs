using System.ComponentModel.DataAnnotations.Schema;

namespace DroneBuilder.Domain.Entities;

public class OrderItem : AuditableEntity
{
    private Product? _legacyProduct;

    public Guid ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }
    public decimal PriceAtPurchase { get; set; }
    public string CurrencyCode { get; set; } = "USD";

    [NotMapped]
    public Product? Product
    {
        get => ProductVariant?.Product ?? _legacyProduct;
        set => _legacyProduct = value;
    }
}
