using System.Text.Json;
using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Models.NotificationModels;
using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Infrastructure.MessageBroker.Services;

public class NotificationService(ApplicationDbContext context) : INotificationService
{
    public async Task SendNotificationAsync(NotificationMessageModel notification)
    {
        var outboxMessage = new Message
        {
            Type = notification.GetType().Name,
            Payload = JsonSerializer.Serialize(notification)
        };

        await context.Messages.AddAsync(outboxMessage);
    }
}