using DroneBuilder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DroneBuilder.Infrastructure.Services;

public sealed class ExpiredInventoryReservationHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<ExpiredInventoryReservationHostedService> logger)
    : BackgroundService
{
    private const int BatchSize = 100;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(InventoryReservationPolicy.CleanupInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            await ReleaseExpiredReservationsAsync(stoppingToken);

            if (!await timer.WaitForNextTickAsync(stoppingToken))
            {
                break;
            }
        }
    }

    private async Task ReleaseExpiredReservationsAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
            ApplicationDbContext dbContext =
                scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            DateTime now = DateTime.UtcNow;
            List<InventoryReservation> reservations = await dbContext.InventoryReservations
                .Include(reservation => reservation.WarehouseItem)
                .Include(reservation => reservation.CartItem)
                .Where(reservation =>
                    reservation.Status == InventoryReservationStatus.Active &&
                    reservation.ExpiresAt <= now)
                .OrderBy(reservation => reservation.ExpiresAt)
                .Take(BatchSize)
                .ToListAsync(cancellationToken);

            foreach (InventoryReservation reservation in reservations)
            {
                reservation.WarehouseItem!.ExpireReservation(reservation, now);
            }

            if (reservations.Count > 0)
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                logger.LogInformation(
                    "Released {ReservationCount} expired inventory reservations.",
                    reservations.Count);
            }
        }
        catch (DbUpdateConcurrencyException exception)
        {
            logger.LogWarning(
                exception,
                "Inventory reservations changed concurrently; they will be retried.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to release expired inventory reservations.");
        }
    }
}
