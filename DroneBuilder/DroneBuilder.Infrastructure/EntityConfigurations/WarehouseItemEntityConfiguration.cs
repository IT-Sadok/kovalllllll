using DroneBuilder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DroneBuilder.Infrastructure.EntityConfigurations;

public class WarehouseItemEntityConfiguration : IEntityTypeConfiguration<WarehouseItem>
{
    public void Configure(EntityTypeBuilder<WarehouseItem> builder)
    {
        builder.HasKey(wi => wi.Id);

        builder.Property(wi => wi.Quantity)
            .IsRequired();

        builder.Property(wi => wi.ReservedQuantity)
            .IsRequired();

        builder.Property(wi => wi.Version)
            .IsConcurrencyToken();

        builder.HasOne(wi => wi.Warehouse)
            .WithMany(w => w.WarehouseItems)
            .HasForeignKey(wi => wi.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(wi => wi.ProductVariant)
            .WithMany(v => v.WarehouseItems)
            .HasForeignKey(wi => wi.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(wi => wi.ProductId);
        builder.Ignore(wi => wi.Product);

        builder.HasIndex(wi => new { wi.WarehouseId, wi.ProductVariantId })
            .IsUnique();

        builder.ToTable(table =>
        {
            table.HasCheckConstraint("CK_WarehouseItems_Quantity_NonNegative", "\"Quantity\" >= 0");
            table.HasCheckConstraint("CK_WarehouseItems_ReservedQuantity_NonNegative", "\"ReservedQuantity\" >= 0");
            table.HasCheckConstraint("CK_WarehouseItems_ReservedWithinQuantity",
                "\"ReservedQuantity\" <= \"Quantity\"");
        });
    }
}
