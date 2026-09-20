namespace DroneBuilder.Domain.Events.CartEvents;

public class UpdatedCartItemQuantityEvent(Guid userId, Guid productId, int quantity) : DomainEvent
{
    public Guid UserId { get; init; } = userId;
    public Guid ProductId { get; init; } = productId;

    /// <summary>The new absolute quantity, not a delta. Zero means the item was removed.</summary>
    public int Quantity { get; init; } = quantity;
}
