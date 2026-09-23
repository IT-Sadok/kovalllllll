namespace DroneBuilder.Infrastructure.Options;

public class ResendOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;

    public string ConfirmationUrl { get; set; } = string.Empty;
}
