using System.Text.Json;
using DroneBuilder.Application.Abstractions;
using DroneBuilder.Domain.Events.ImageEvents;
using DroneBuilder.Infrastructure.Common;
using Microsoft.Extensions.Logging;

namespace DroneBuilder.Infrastructure.MessageBroker.Handlers.ImageHandlers;

public class ImageUploadedEventHandler(ILogger<ImageUploadedEventHandler> logger) : IEventHandler
{
    public string EventType => typeof(ImageUploadedEvent).FullName!;

    public async Task HandleAsync(string json, CancellationToken cancellationToken = default)
    {
        var @event = JsonSerializer.Deserialize<ImageUploadedEvent>(json, JsonSettings.JsonSerializerOptions);

        if (@event == null)
        {
            logger.LogWarning("Invalid ImageUploadedEvent");
            return;
        }

        logger.LogInformation(
            "Image uploaded! ImageId={ImageId}, ProductId={ProductId}",
            @event.ImageId,
            @event.ProductId
        );

        await Task.CompletedTask;
    }
}