using System.Text.Json;
using DroneBuilder.Application.Abstractions;
using DroneBuilder.Domain.Events.OrderEvents;
using DroneBuilder.Infrastructure.Common;
using Microsoft.Extensions.Logging;

namespace DroneBuilder.Infrastructure.MessageBroker.Handlers.OrderHandlers;

public class OrderCreatedEventHandler(ILogger<OrderCreatedEventHandler> logger) : IEventHandler
{
    public string EventType => typeof(OrderCreatedEvent).FullName!;

    public async Task HandleAsync(string json, CancellationToken cancellationToken = default)
    {
        var @event = JsonSerializer.Deserialize<OrderCreatedEvent>(json, JsonSettings.JsonSerializerOptions);

        if (@event == null)
        {
            logger.LogWarning("Invalid OrderCreatedEvent");
            return;
        }

        logger.LogInformation(
            "Order created! OrderId={OrderId}, UserId={UserId}",
            @event.OrderId,
            @event.UserId
        );

        await Task.CompletedTask;
    }
}