using DroneBuilder.Domain.Events;

namespace DroneBuilder.Application.Abstractions;

public interface IOutboxEventService
{
    Task PublishEventAsync<TEvent>(TEvent @event, string queueName, CancellationToken ct = default)
        where TEvent : DomainEvent;
}