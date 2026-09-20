namespace DroneBuilder.Infrastructure.Options;

public class ResendOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;

    /// <summary>
    /// Absolute URL of the page that receives the confirmation link, for example
    /// https://example.com/api/users/confirm-email. The userId and token are appended as query parameters.
    /// </summary>
    public string ConfirmationUrl { get; set; } = string.Empty;
}
