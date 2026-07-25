using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Tests.TestSupport;

internal static class ReservationTestData
{
    public static InventoryReservation Attach(
        Cart cart,
        CartItem cartItem,
        WarehouseItem warehouseItem,
        DateTime? expiresAt = null)
    {
        warehouseItem.ReservedQuantity = 0;
        cartItem.Cart = cart;
        cartItem.CartId = cart.Id;

        InventoryReservation reservation = warehouseItem.CreateReservation(
            cart.Id,
            cartItem.Id,
            cartItem.Quantity,
            expiresAt ?? DateTime.UtcNow.AddHours(1));

        reservation.Cart = cart;
        reservation.CartItem = cartItem;
        cartItem.Reservation = reservation;
        cart.InventoryReservations.Add(reservation);
        return reservation;
    }
}
