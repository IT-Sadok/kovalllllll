namespace DroneBuilder.Domain.Entities;

public class CartItem
{
    public Guid Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }

    /// <summary>
    /// When the stock behind this item was last taken out of the warehouse. Reservations expire so an
    /// abandoned cart cannot hold stock forever; it is refreshed whenever the quantity grows.
    /// </summary>
    public DateTime ReservedAt { get; set; } = DateTime.UtcNow;
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public Guid CartId { get; set; }
    public Cart? Cart { get; set; }
}
