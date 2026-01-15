using System.Text.Json;
using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Models.NotificationModels;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Events;

namespace DroneBuilder.Infrastructure.MessageBroker.Services;

public class OutboxEventService(ApplicationDbContext context) : IOutboxEventService
{
    public async Task StoreEventAsync<TEvent>(TEvent @event, string queueName,
        CancellationToken cancellationToken = default)
        where TEvent : DomainEvent
    {
        var message = new Message
        {
            Type = typeof(TEvent).FullName!,
            Payload = JsonSerializer.Serialize(@event, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }),
            QueueName = queueName
        };

        await context.Messages.AddAsync(message, cancellationToken);
    }
}