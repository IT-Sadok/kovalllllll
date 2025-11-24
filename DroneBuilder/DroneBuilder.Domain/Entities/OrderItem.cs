namespace DroneBuilder.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }
    public decimal PriceAtPurchase { get; set; }
}