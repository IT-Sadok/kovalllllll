namespace DroneBuilder.Application.Options;

public class MessageQueuesConfiguration
{
    public string UserQueue { get; set; } = string.Empty;
    public string CartQueue { get; set; } = string.Empty;
    public string OrderQueue { get; set; } = string.Empty;
    public string ImageQueue { get; set; } = string.Empty;
    public string ProductQueue { get; set; } = string.Empty;
    public string WarehouseQueue { get; set; } = string.Empty;
}