namespace DroneBuilder.Application.Options;

public class CartReservationOptions
{
    /// <summary>How long stock stays reserved for an item sitting in a cart.</summary>
    public int TimeToLiveMinutes { get; set; } = 60;

    /// <summary>How often expired reservations are swept back into the warehouse.</summary>
    public int SweepIntervalMinutes { get; set; } = 5;
}
