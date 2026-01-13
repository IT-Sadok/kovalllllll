namespace DroneBuilder.Domain.Events;

public abstract class DomainEvent
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Type { get; init; }
    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;

    protected DomainEvent()
    {
        Type = GetType().FullName!;
    }
}