namespace DroneBuilder.Application.Models.NotificationModels;

public class NotificationMessageModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public NotificationType Type { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string UserId { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}