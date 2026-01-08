using DroneBuilder.Application.Models.NotificationModels;

namespace DroneBuilder.Application.Abstractions;

public interface INotificationService
{
    Task SendNotificationAsync(NotificationMessageModel notification);
}