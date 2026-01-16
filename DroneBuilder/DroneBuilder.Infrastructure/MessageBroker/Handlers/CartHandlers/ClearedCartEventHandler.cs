using System.Text.Json;
using DroneBuilder.Application.Abstractions;
using DroneBuilder.Domain.Events.CartEvents;
using DroneBuilder.Infrastructure.Common;
using Microsoft.Extensions.Logging;

namespace DroneBuilder.Infrastructure.MessageBroker.Handlers.CartHandlers;

public class ClearedCartEventHandler(ILogger<ClearedCartEventHandler> logger) : IEventHandler
{
    public string EventType => typeof(ClearedCartEvent).FullName!;

    public async Task HandleAsync(string json, CancellationToken cancellationToken = default)
    {
        var @event = JsonSerializer.Deserialize<ClearedCartEvent>(json,
            JsonSettings.JsonSerializerOptions);
        if (@event == null)
        {
            logger.LogWarning("Invalid ClearedCartEvent");
            return;
        }

        logger.LogInformation(
            "Cart cleared! UserId={UserId}",
            @event.UserId
        );

        await Task.CompletedTask;
    }
}