namespace DroneBuilder.Domain.Entities;

public static class InventoryReservationPolicy
{
    public static readonly TimeSpan DefaultLifetime = TimeSpan.FromMinutes(30);
    public static readonly TimeSpan CleanupInterval = TimeSpan.FromMinutes(1);

    public static DateTime NewExpiration(DateTime now) => now.Add(DefaultLifetime);
}
