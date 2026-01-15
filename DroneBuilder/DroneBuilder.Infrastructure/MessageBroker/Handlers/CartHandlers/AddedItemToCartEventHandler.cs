using System.Text.Json;
using DroneBuilder.Application.Abstractions;
using DroneBuilder.Domain.Events.CartEvents;
using DroneBuilder.Infrastructure.Common;
using Microsoft.Extensions.Logging;

namespace DroneBuilder.Infrastructure.MessageBroker.Handlers.CartHandlers;

public class AddedItemToCartEventHandler(ILogger<AddedItemToCartEventHandler> logger) : IEventHandler
{
    public string EventType => typeof(AddedItemToCartEvent).FullName!;

    public async Task HandleAsync(string json, CancellationToken cancellationToken = default)
    {
        var @event = JsonSerializer.Deserialize<AddedItemToCartEvent>(json,
            JsonSettings.JsonSerializerOptions);
        if (@event == null)
        {
            logger.LogWarning("Invalid AddedItemToCartEvent");
            return;
        }

        logger.LogInformation(
            "Item added to cart! ProductId={ProductId} with name={ProductName}, Quantity={Quantity}",
            @event.ProductId,
            @event.ProductName,
            @event.Quantity
        );

        await Task.CompletedTask;
    }
}