namespace DroneBuilder.Domain.Events.OrderEvents;

public class OrderCreatedEvent(Guid orderId, Guid userId) : DomainEvent
{
    public Guid OrderId { get; init; } = orderId;
    public Guid UserId { get; init; } = userId;
}