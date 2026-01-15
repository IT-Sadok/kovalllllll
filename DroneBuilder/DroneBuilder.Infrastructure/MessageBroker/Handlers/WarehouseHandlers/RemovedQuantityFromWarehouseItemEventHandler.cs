using System.Text.Json;
using DroneBuilder.Application.Abstractions;
using DroneBuilder.Domain.Events.WarehouseEvents;
using DroneBuilder.Infrastructure.Common;
using Microsoft.Extensions.Logging;

namespace DroneBuilder.Infrastructure.MessageBroker.Handlers.WarehouseHandlers;

public class RemovedQuantityFromWarehouseItemEventHandler(ILogger<RemovedQuantityFromWarehouseItemEventHandler> logger)
    : IEventHandler
{
    public string EventType => typeof(RemovedQuantityFromWarehouseItemEvent).FullName!;

    public async Task HandleAsync(string json, CancellationToken cancellationToken = default)
    {
        var @event = JsonSerializer.Deserialize<RemovedQuantityFromWarehouseItemEvent>(json,
            JsonSettings.JsonSerializerOptions);
        if (@event == null)
        {
            logger.LogWarning("Invalid RemovedQuantityFromWarehouseItemEvent");
            return;
        }

        logger.LogInformation(
            "Quantity removed from warehouse item! WarehouseItemId={WarehouseItemId}, Quantity={QuantityRemoved}",
            @event.WarehouseItemId,
            @event.QuantityRemoved
        );

        await Task.CompletedTask;
    }
}