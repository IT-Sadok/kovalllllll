using DroneBuilder.Domain.Events;

namespace DroneBuilder.Application.Abstractions;

public interface IEventHandler
{
    string EventType { get; }
    Task HandleAsync(string json, CancellationToken cancellationToken = default);
}