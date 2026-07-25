namespace DroneBuilder.Domain.Entities;

public class Cart : AuditableEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public ICollection<CartItem> CartItems { get; set; } = [];
    public ICollection<InventoryReservation> InventoryReservations { get; set; } = [];
}
