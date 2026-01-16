namespace DroneBuilder.Domain.Events.WarehouseEvents;

public class AddedQuantityToWarehouseItemEvent (Guid warehouseItemId, int quantityAdded) : DomainEvent
{
    public Guid WarehouseItemId { get; } = warehouseItemId;
    public int QuantityAdded { get; } = quantityAdded;
}