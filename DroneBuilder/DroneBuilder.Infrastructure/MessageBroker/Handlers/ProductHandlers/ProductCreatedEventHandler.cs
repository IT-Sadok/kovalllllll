using System.Text.Json;
using DroneBuilder.Application.Abstractions;
using DroneBuilder.Domain.Events.ProductEvents;
using DroneBuilder.Infrastructure.Common;
using Microsoft.Extensions.Logging;

namespace DroneBuilder.Infrastructure.MessageBroker.Handlers.ProductHandlers;

public class ProductCreatedEventHandler(ILogger<ProductCreatedEventHandler> logger) : IEventHandler
{
    public string EventType => typeof(ProductCreatedEvent).FullName!;

    public async Task HandleAsync(string json, CancellationToken cancellationToken = default)
    {
        var @event = JsonSerializer.Deserialize<ProductCreatedEvent>(json,
            JsonSettings.JsonSerializerOptions);
        if (@event == null)
        {
            logger.LogWarning("Invalid ProductCreatedEvent");
            return;
        }

        logger.LogInformation(
            "Product created! ProductId={ProductId}",
            @event.ProductId
        );

        await Task.CompletedTask;
    }
}