namespace DroneBuilder.Domain.Entities;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public Status Status { get; set; } = Status.New;
    public ICollection<OrderItem> OrderItems { get; set; } = [];
    public decimal TotalPrice { get; set; }
    public string ShippingDetails { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}