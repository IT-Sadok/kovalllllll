namespace DroneBuilder.Domain.Entities;

public class Message
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Type { get; set; } = null!;
    public string Payload { get; set; } = null!;
    public string QueueName { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public string? Error { get; set; }
    public int RetryCount { get; set; }
}