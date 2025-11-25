namespace DroneBuilder.Domain.Entities;

public class CartItem
{
    public Guid Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public Guid CartId { get; set; }
    public Cart? Cart { get; set; }
}