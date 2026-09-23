namespace DroneBuilder.Domain.Events.CartEvents;

public class UpdatedCartItemQuantityEvent(Guid userId, Guid productId, int quantity) : DomainEvent
{
    public Guid UserId { get; init; } = userId;
    public Guid ProductId { get; init; } = productId;

    public int Quantity { get; init; } = quantity;
}
