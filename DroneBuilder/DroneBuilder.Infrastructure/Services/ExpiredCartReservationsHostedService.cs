using DroneBuilder.Application.Options;
using DroneBuilder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DroneBuilder.Infrastructure.Services;

/// <summary>
/// Stock leaves the warehouse the moment an item goes into a cart. Without this sweep an abandoned
/// cart would hold that stock forever, and one customer could take the whole warehouse out of sale.
/// </summary>
public class ExpiredCartReservationsHostedService(
    IServiceProvider serviceProvider,
    CartReservationOptions options,
    ILogger<ExpiredCartReservationsHostedService> logger) : BackgroundService
{
    private const int BatchSize = 200;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        TimeSpan interval = TimeSpan.FromMinutes(options.SweepIntervalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                int released = await ReleaseExpiredReservationsAsync(stoppingToken);

                if (released > 0)
                {
                    logger.LogInformation("Released {Count} expired cart reservations", released);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error releasing expired cart reservations");
            }

            try
            {
                await Task.Delay(interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task<int> ReleaseExpiredReservationsAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        DateTime cutoff = DateTime.UtcNow.AddMinutes(-options.TimeToLiveMinutes);

        await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        // SKIP LOCKED keeps a second instance from returning the same stock twice.
        // Nothing may be composed onto this query: EF would wrap it and FOR UPDATE is invalid there.
        List<CartItem> expiredItems = await context.CartItems
            .FromSql(
                $"""
                 SELECT * FROM "CartItems"
                 WHERE "ReservedAt" < {cutoff}
                 ORDER BY "ReservedAt"
                 LIMIT {BatchSize}
                 FOR UPDATE SKIP LOCKED
                 """)
            .ToListAsync(cancellationToken);

        if (expiredItems.Count == 0)
        {
            await transaction.CommitAsync(cancellationToken);
            return 0;
        }

        var productIds = expiredItems.Select(ci => ci.ProductId).Distinct().ToList();

        List<WarehouseItem> warehouseItems = await context.WarehouseItems
            .Where(wi => productIds.Contains(wi.ProductId))
            .ToListAsync(cancellationToken);

        Dictionary<Guid, WarehouseItem> warehouseByProduct = warehouseItems.ToDictionary(wi => wi.ProductId);

        foreach (CartItem item in expiredItems)
        {
            if (warehouseByProduct.TryGetValue(item.ProductId, out WarehouseItem? warehouseItem))
            {
                warehouseItem.Quantity += item.Quantity;
            }
            else
            {
                // The product was delisted, which already removed its warehouse record. There is
                // nothing to give the stock back to, so the item is only dropped from the cart.
                logger.LogWarning(
                    "No warehouse record for product {ProductId}; dropping the expired reservation without restocking",
                    item.ProductId);
            }
        }

        context.CartItems.RemoveRange(expiredItems);

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return expiredItems.Count;
    }
}
