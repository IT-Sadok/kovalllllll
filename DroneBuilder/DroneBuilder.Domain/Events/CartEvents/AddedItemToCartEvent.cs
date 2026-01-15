namespace DroneBuilder.Domain.Events.CartEvents;

public class AddedItemToCartEvent(Guid userId, Guid productId, string productName, int quantity) : DomainEvent
{
    public Guid UserId { get; } = userId;
    public Guid ProductId { get; } = productId;
    public string ProductName { get; init; } = productName;
    public int Quantity { get; } = quantity;
}