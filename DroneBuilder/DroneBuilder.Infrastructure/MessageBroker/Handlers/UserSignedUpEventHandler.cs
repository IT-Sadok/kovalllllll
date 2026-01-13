using System.Text.Json;
using DroneBuilder.Application.Abstractions;
using DroneBuilder.Domain.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DroneBuilder.Infrastructure.MessageBroker.Handlers;

public class UserSignedUpEventHandler(ILogger<UserSignedUpEventHandler> logger) : IEventHandler<UserSignedUpEvent>
{
    public async Task HandleAsync(UserSignedUpEvent @event, CancellationToken ct = default)
    {
        logger.LogInformation(
            "User signed up! UserId={UserId}, Email={Email}",
            @event.UserId,
            @event.Email
        );

        await Task.CompletedTask;
    }
}