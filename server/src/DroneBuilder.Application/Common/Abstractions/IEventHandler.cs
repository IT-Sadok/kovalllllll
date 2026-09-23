namespace DroneBuilder.Application.Common.Abstractions;

public interface IEventHandler
{
    string EventType { get; }
    Task HandleAsync(string json, CancellationToken cancellationToken = default);
}
