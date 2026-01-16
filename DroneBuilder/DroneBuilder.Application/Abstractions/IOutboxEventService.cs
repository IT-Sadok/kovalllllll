using DroneBuilder.Domain.Events;

namespace DroneBuilder.Application.Abstractions;

public interface IOutboxEventService
{
    Task StoreEventAsync<TEvent>(TEvent @event, string queueName, CancellationToken cancellationToken = default)
        where TEvent : DomainEvent;
}