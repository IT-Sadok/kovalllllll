using DroneBuilder.Domain.Events;

namespace DroneBuilder.Application.Abstractions;

public interface IEventHandler<TEvent> where TEvent : DomainEvent
{
    Task HandleAsync(TEvent @event, CancellationToken ct = default);
}