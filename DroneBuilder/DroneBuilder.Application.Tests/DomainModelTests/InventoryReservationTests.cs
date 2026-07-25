using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Tests.DomainModelTests;

public class InventoryReservationTests
{
    [Fact]
    public void ReservationLifecycle_TracksOwnerAndReleasesStock()
    {
        var warehouseItem = new WarehouseItem { Quantity = 10 };
        Guid cartId = Guid.NewGuid();
        Guid cartItemId = Guid.NewGuid();
        DateTime now = DateTime.UtcNow;

        InventoryReservation reservation = warehouseItem.CreateReservation(
            cartId,
            cartItemId,
            4,
            now.AddMinutes(30));

        Assert.Equal(4, warehouseItem.ReservedQuantity);
        Assert.Equal(cartId, reservation.CartId);
        Assert.Equal(cartItemId, reservation.CartItemId);
        Assert.Equal(InventoryReservationStatus.Active, reservation.Status);

        warehouseItem.ReleaseReservation(reservation, now.AddMinutes(1));

        Assert.Equal(0, warehouseItem.ReservedQuantity);
        Assert.Equal(InventoryReservationStatus.Released, reservation.Status);
        Assert.Null(reservation.CartItemId);
        Assert.NotNull(reservation.CompletedAt);
    }

    [Fact]
    public void CommitReservation_DecrementsPhysicalAndReservedQuantities()
    {
        var warehouseItem = new WarehouseItem { Quantity = 10 };
        DateTime now = DateTime.UtcNow;
        InventoryReservation reservation = warehouseItem.CreateReservation(
            Guid.NewGuid(),
            Guid.NewGuid(),
            3,
            now.AddMinutes(30));

        warehouseItem.CommitReservation(reservation, now.AddMinutes(1));

        Assert.Equal(7, warehouseItem.Quantity);
        Assert.Equal(0, warehouseItem.ReservedQuantity);
        Assert.Equal(InventoryReservationStatus.Committed, reservation.Status);
    }
}
