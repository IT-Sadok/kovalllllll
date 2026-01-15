namespace DroneBuilder.Application.Options;

public class QueueConfiguration
{
    public string Name { get; set; } = string.Empty;
    public bool Durable { get; set; } = true;
    public bool AutoDelete { get; set; } = false;
    public bool Exclusive { get; set; } = false;
    public int MaxRetryCount { get; set; } = 3;
    public int PrefetchCount { get; set; } = 10;
    public Dictionary<string, object>? Arguments { get; set; }
}