namespace DroneBuilder.Domain.Events.ProductEvents;

public class ProductCreatedEvent(Guid productId) : DomainEvent
{
    public Guid ProductId { get; init; } = productId;
}