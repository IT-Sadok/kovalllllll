namespace DroneBuilder.Domain.Entities;

public class Cart
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public ICollection<CartItem> CartItems { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}