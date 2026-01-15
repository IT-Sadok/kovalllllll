using System.Text.Json;
using DroneBuilder.Application.Abstractions;
using DroneBuilder.Domain.Events.UserEvents;
using DroneBuilder.Infrastructure.Common;
using Microsoft.Extensions.Logging;

namespace DroneBuilder.Infrastructure.MessageBroker.Handlers.UserHandlers;

public class UserSignedInEventHandler(ILogger<UserSignedInEventHandler> logger) : IEventHandler
{
    public string EventType => typeof(UserSignedInEvent).FullName!;

    public async Task HandleAsync(string json, CancellationToken cancellationToken = default)
    {
        var @event = JsonSerializer.Deserialize<UserSignedInEvent>(json,
            JsonSettings.JsonSerializerOptions);

        if (@event == null)
        {
            logger.LogWarning("Invalid UserSignedInEvent");
            return;
        }

        logger.LogInformation(
            "User signed in! UserId={UserId}, Email={Email}",
            @event.UserId,
            @event.Email
        );

        await Task.CompletedTask;
    }
}