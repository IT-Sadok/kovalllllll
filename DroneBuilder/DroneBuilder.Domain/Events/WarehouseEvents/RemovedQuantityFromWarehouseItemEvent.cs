using Microsoft.Extensions.Logging;

namespace DroneBuilder.Domain.Events.WarehouseEvents;

public class RemovedQuantityFromWarehouseItemEvent(Guid warehouseItemId, int quantityRemoved) : DomainEvent
{
    public Guid WarehouseItemId { get; } = warehouseItemId;
    public int QuantityRemoved { get; } = quantityRemoved;
}