namespace DroneBuilder.Application.Options;

public class CartReservationOptions
{
    public int TimeToLiveMinutes { get; set; } = 60;

    public int SweepIntervalMinutes { get; set; } = 5;
}
