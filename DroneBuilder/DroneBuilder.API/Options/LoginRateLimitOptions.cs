namespace DroneBuilder.API.Options;

public class LoginRateLimitOptions
{
    public int PermitLimit { get; set; } = 20;
    public int WindowInMinutes { get; set; } = 15;
}
