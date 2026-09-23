using System.Text.Json;
using DroneBuilder.Application.Common.Abstractions;
using DroneBuilder.Domain.Events.CartEvents;
using DroneBuilder.Infrastructure.Common;
using Microsoft.Extensions.Logging;

namespace DroneBuilder.Infrastructure.MessageBroker.Handlers.CartHandlers;

public class UpdatedCartItemQuantityEventHandler(ILogger<UpdatedCartItemQuantityEventHandler> logger) : IEventHandler
{
    public string EventType => typeof(UpdatedCartItemQuantityEvent).FullName!;

    public async Task HandleAsync(string json, CancellationToken cancellationToken = default)
    {
        UpdatedCartItemQuantityEvent? @event =
            JsonSerializer.Deserialize<UpdatedCartItemQuantityEvent>(json, JsonSettings.JsonSerializerOptions);

        if (@event == null)
        {
            logger.LogWarning("Invalid UpdatedCartItemQuantityEvent");
            return;
        }

        logger.LogInformation(
            "Cart item quantity updated! UserId={UserId}, ProductId={ProductId}, Quantity={Quantity}",
            @event.UserId,
            @event.ProductId,
            @event.Quantity
        );

        await Task.CompletedTask;
    }
}
