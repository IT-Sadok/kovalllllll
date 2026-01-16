namespace DroneBuilder.Domain.Events.CartEvents;

public class ClearedCartEvent(Guid userId) : DomainEvent
{
    public Guid UserId { get; } = userId;
}