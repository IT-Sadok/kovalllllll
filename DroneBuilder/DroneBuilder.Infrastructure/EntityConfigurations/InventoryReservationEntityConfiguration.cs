using DroneBuilder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DroneBuilder.Infrastructure.EntityConfigurations;

public class InventoryReservationEntityConfiguration : IEntityTypeConfiguration<InventoryReservation>
{
    public void Configure(EntityTypeBuilder<InventoryReservation> builder)
    {
        builder.HasKey(reservation => reservation.Id);
        builder.Property(reservation => reservation.Status)
            .HasConversion<int>();
        builder.Ignore(reservation => reservation.IsActive);

        builder.HasOne(reservation => reservation.WarehouseItem)
            .WithMany(item => item.Reservations)
            .HasForeignKey(reservation => reservation.WarehouseItemId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(reservation => reservation.Cart)
            .WithMany(cart => cart.InventoryReservations)
            .HasForeignKey(reservation => reservation.CartId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(reservation => reservation.CartItem)
            .WithOne(item => item.Reservation)
            .HasForeignKey<InventoryReservation>(reservation => reservation.CartItemId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(reservation => reservation.CartItemId)
            .HasFilter("\"CartItemId\" IS NOT NULL")
            .IsUnique();
        builder.HasIndex(reservation => new { reservation.Status, reservation.ExpiresAt });
        builder.ToTable(table =>
        {
            table.HasCheckConstraint(
                "CK_InventoryReservations_Quantity_Positive",
                "\"Quantity\" > 0");
            table.HasCheckConstraint(
                "CK_InventoryReservations_ActiveCartItem",
                "\"Status\" <> 0 OR \"CartItemId\" IS NOT NULL");
            table.HasCheckConstraint(
                "CK_InventoryReservations_CompletionState",
                "(\"Status\" = 0 AND \"CompletedAt\" IS NULL) OR " +
                "(\"Status\" <> 0 AND \"CompletedAt\" IS NOT NULL)");
        });
    }
}
