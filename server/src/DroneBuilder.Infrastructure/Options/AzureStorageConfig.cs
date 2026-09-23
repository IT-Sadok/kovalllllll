namespace DroneBuilder.Infrastructure.Options;

public class AzureStorageConfig
{
    public string ConnectionString { get; set; } = string.Empty;
    public string ContainerName { get; set; } = string.Empty;
}
