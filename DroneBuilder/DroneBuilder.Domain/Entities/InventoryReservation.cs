namespace DroneBuilder.Domain.Entities;

public class InventoryReservation : AuditableEntity
{
    public Guid WarehouseItemId { get; set; }
    public WarehouseItem? WarehouseItem { get; set; }
    public Guid CartId { get; set; }
    public Cart? Cart { get; set; }
    public Guid? CartItemId { get; set; }
    public CartItem? CartItem { get; set; }
    public int Quantity { get; set; }
    public InventoryReservationStatus Status { get; set; } = InventoryReservationStatus.Active;
    public DateTime ExpiresAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public bool IsActive => Status == InventoryReservationStatus.Active;
}
