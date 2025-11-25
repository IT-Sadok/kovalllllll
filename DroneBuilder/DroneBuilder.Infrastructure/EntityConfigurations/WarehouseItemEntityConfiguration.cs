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

        builder.Property(wi => wi.AvailableQuantity)
            .IsRequired();

        builder.HasOne(wi => wi.Warehouse)
            .WithMany(w => w.WarehouseItems)
            .HasForeignKey(wi => wi.WarehouseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(wi => wi.Product)
            .WithOne()
            .HasForeignKey<WarehouseItem>(wi => wi.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}