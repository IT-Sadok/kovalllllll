namespace DroneBuilder.Domain.Events;

public abstract class DomainEvent
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
}