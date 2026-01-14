using System.Text.Json;
using DroneBuilder.Application.Abstractions;
using DroneBuilder.Domain.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DroneBuilder.Infrastructure.MessageBroker.Handlers;

public class UserSignedUpEventHandler(ILogger<UserSignedUpEventHandler> logger) : IEventHandler
{
    public string EventType => typeof(UserSignedUpEvent).FullName!;

    public async Task HandleAsync(string json, CancellationToken ct = default)
    {
        var @event = JsonSerializer.Deserialize<UserSignedUpEvent>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (@event == null)
        {
            logger.LogWarning("Invalid UserSignedUpEvent");
            return;
        }

        logger.LogInformation(
            "User signed up! UserId={UserId}, Email={Email}",
            @event.UserId,
            @event.Email
        );

        await Task.CompletedTask;
    }
}