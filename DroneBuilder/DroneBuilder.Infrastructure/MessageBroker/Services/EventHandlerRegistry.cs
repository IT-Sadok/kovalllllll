using DroneBuilder.Application.Abstractions;
using DroneBuilder.Domain.Events;
using Microsoft.Extensions.DependencyInjection;

namespace DroneBuilder.Infrastructure.MessageBroker.Services;

public class EventHandlerRegistry
{
    private readonly Dictionary<string, (Type HandlerType, Type EventType)> _handlers = new();

    public void RegisterInstance(string eventTypeName, Type handlerType, Type eventType)
    {
        _handlers[eventTypeName] = (handlerType, eventType);
    }

    public void Register<THandler, TEvent>()
        where THandler : IEventHandler<TEvent>
        where TEvent : DomainEvent
    {
        var eventType = typeof(TEvent);
        var handlerType = typeof(THandler);
        _handlers[eventType.FullName!] = (handlerType, eventType);
    }

    public async Task HandleAsync(string eventTypeName, string json, IServiceScope scope, CancellationToken ct)
    {
        if (!_handlers.TryGetValue(eventTypeName, out var tuple))
        {
            throw new InvalidOperationException($"No handler registered for event type: {eventTypeName}");
        }

        var (handlerType, eventType) = tuple;

        var @event = System.Text.Json.JsonSerializer.Deserialize(json, eventType, 
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        if (@event == null)
            throw new InvalidOperationException($"Failed to deserialize event: {eventTypeName}");

        var handler = scope.ServiceProvider.GetRequiredService(handlerType);
        
        var handleMethod = handlerType.GetMethod(nameof(IEventHandler<DomainEvent>.HandleAsync));
        if (handleMethod == null)
            throw new InvalidOperationException($"HandleAsync method not found on {handlerType.Name}");

        if (handleMethod.Invoke(handler, [@event, ct]) is Task task)
            await task;
    }

    public bool CanHandle(string eventType) => _handlers.ContainsKey(eventType);
}