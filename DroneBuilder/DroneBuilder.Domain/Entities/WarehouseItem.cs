using System.ComponentModel.DataAnnotations.Schema;

namespace DroneBuilder.Domain.Entities;

public class WarehouseItem : AuditableEntity
{
    private Guid _legacyProductId;
    private Product? _legacyProduct;

    public Guid WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }
    public Guid ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }
    public int Quantity { get; set; }
    public int ReservedQuantity { get; set; }
    public int Version { get; set; }
    public ICollection<InventoryReservation> Reservations { get; set; } = [];

    [NotMapped]
    public int AvailableQuantity => Quantity - ReservedQuantity;

    [NotMapped]
    public Guid ProductId
    {
        get => ProductVariant?.ProductId ?? _legacyProductId;
        set => _legacyProductId = value;
    }

    [NotMapped]
    public Product? Product
    {
        get => ProductVariant?.Product ?? _legacyProduct;
        set => _legacyProduct = value;
    }

    public void AttachVariant(ProductVariant variant)
    {
        ProductVariant = variant;
        ProductVariantId = variant.Id;
        _legacyProductId = variant.ProductId;
        _legacyProduct = variant.Product;
    }

    public void AddStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity));
        }

        checked
        {
            Quantity += quantity;
        }

        Version++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveStock(int quantity)
    {
        if (quantity <= 0 || Quantity - quantity < ReservedQuantity)
        {
            throw new InvalidOperationException(
                "Stock cannot be reduced below the reserved quantity.");
        }

        Quantity -= quantity;
        Version++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reserve(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity));
        }

        if (AvailableQuantity < quantity)
        {
            throw new InvalidOperationException("Not enough available stock.");
        }

        ReservedQuantity += quantity;
        Version++;
    }

    public InventoryReservation CreateReservation(
        Guid cartId,
        Guid cartItemId,
        int quantity,
        DateTime expiresAt)
    {
        if (expiresAt <= DateTime.UtcNow)
        {
            throw new ArgumentOutOfRangeException(nameof(expiresAt));
        }

        Reserve(quantity);

        var reservation = new InventoryReservation
        {
            WarehouseItemId = Id,
            WarehouseItem = this,
            CartId = cartId,
            CartItemId = cartItemId,
            Quantity = quantity,
            ExpiresAt = expiresAt
        };

        Reservations.Add(reservation);
        return reservation;
    }

    public void ChangeReservation(
        InventoryReservation reservation,
        int newQuantity,
        DateTime expiresAt)
    {
        EnsureActiveReservation(reservation);

        if (newQuantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(newQuantity));
        }

        if (expiresAt <= DateTime.UtcNow)
        {
            throw new ArgumentOutOfRangeException(nameof(expiresAt));
        }

        int difference = newQuantity - reservation.Quantity;
        if (difference > 0)
        {
            Reserve(difference);
        }
        else if (difference < 0)
        {
            Release(-difference);
        }

        reservation.Quantity = newQuantity;
        reservation.ExpiresAt = expiresAt;
        reservation.UpdatedAt = DateTime.UtcNow;
    }

    public void ReleaseReservation(InventoryReservation reservation, DateTime completedAt)
    {
        EnsureActiveReservation(reservation);
        Release(reservation.Quantity);
        reservation.Status = InventoryReservationStatus.Released;
        reservation.CompletedAt = completedAt;
        reservation.UpdatedAt = completedAt;
        DetachFromCartItem(reservation);
    }

    public void ExpireReservation(InventoryReservation reservation, DateTime completedAt)
    {
        EnsureActiveReservation(reservation);
        Release(reservation.Quantity);
        reservation.Status = InventoryReservationStatus.Expired;
        reservation.CompletedAt = completedAt;
        reservation.UpdatedAt = completedAt;
        DetachFromCartItem(reservation);
    }

    public void CommitReservation(InventoryReservation reservation, DateTime completedAt)
    {
        EnsureActiveReservation(reservation);
        CommitReservation(reservation.Quantity);
        reservation.Status = InventoryReservationStatus.Committed;
        reservation.CompletedAt = completedAt;
        reservation.UpdatedAt = completedAt;
        DetachFromCartItem(reservation);
    }

    public void Release(int quantity)
    {
        if (quantity <= 0 || quantity > ReservedQuantity)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity));
        }

        ReservedQuantity -= quantity;
        Version++;
    }

    public void CommitReservation(int quantity)
    {
        if (quantity <= 0 || quantity > ReservedQuantity)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity));
        }

        ReservedQuantity -= quantity;
        Quantity -= quantity;
        Version++;
    }

    private void EnsureActiveReservation(InventoryReservation reservation)
    {
        ArgumentNullException.ThrowIfNull(reservation);

        if (reservation.WarehouseItemId != Id ||
            reservation.Status != InventoryReservationStatus.Active)
        {
            throw new InvalidOperationException("Reservation is not active for this warehouse item.");
        }
    }

    private static void DetachFromCartItem(InventoryReservation reservation)
    {
        CartItem? cartItem = reservation.CartItem;
        reservation.CartItem = null;
        reservation.CartItemId = null;

        if (cartItem is not null)
        {
            cartItem.Reservation = null;
        }
    }
}
