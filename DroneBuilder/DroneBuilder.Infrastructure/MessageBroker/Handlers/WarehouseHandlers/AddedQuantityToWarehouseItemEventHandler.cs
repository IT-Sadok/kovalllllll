using System.Text.Json;
using DroneBuilder.Application.Abstractions;
using DroneBuilder.Domain.Events.WarehouseEvents;
using Microsoft.Extensions.Logging;

namespace DroneBuilder.Infrastructure.MessageBroker.Handlers.WarehouseHandlers;

public class AddedQuantityToWarehouseItemEventHandler(ILogger<AddedQuantityToWarehouseItemEventHandler> logger)
    : IEventHandler
{
    public string EventType => typeof(AddedQuantityToWarehouseItemEvent).FullName!;

    public async Task HandleAsync(string json, CancellationToken cancellationToken = default)
    {
        var @event = JsonSerializer.Deserialize<AddedQuantityToWarehouseItemEvent>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (@event == null)
        {
            logger.LogWarning("Invalid AddedQuantityToWarehouseItemEvent");
            return;
        }

        logger.LogInformation(
            "Quantity added to warehouse item! WarehouseItemId={WarehouseItemId}, Quantity={QuantityAdded}",
            @event.WarehouseItemId,
            @event.QuantityAdded
        );

        await Task.CompletedTask;
    }
}