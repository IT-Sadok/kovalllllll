namespace DroneBuilder.Application.Options;

public class MessageQueuesConfiguration
{
    public QueueConfiguration UserQueue { get; set; } = new();
    public QueueConfiguration CartQueue { get; set; } = new();
    public QueueConfiguration OrderQueue { get; set; } = new();
    public QueueConfiguration ImageQueue { get; set; } = new();
    public QueueConfiguration ProductQueue { get; set; } = new();
    public QueueConfiguration WarehouseQueue { get; set; } = new();
}