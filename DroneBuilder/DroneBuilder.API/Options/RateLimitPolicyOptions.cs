namespace DroneBuilder.API.Options;

public class RateLimitPolicyOptions
{
    public int PermitLimit { get; set; } = 20;
    public int WindowInMinutes { get; set; } = 15;
}
