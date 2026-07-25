using DroneBuilder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DroneBuilder.Infrastructure.EntityConfigurations;

public class OrderItemEntityConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(oi => oi.Id);

        builder.Property(oi => oi.Quantity)
            .IsRequired();

        builder.Property(oi => oi.PriceAtPurchase)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(oi => oi.ProductName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(oi => oi.Sku)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(oi => oi.CurrencyCode)
            .IsRequired()
            .HasMaxLength(3);

        builder.HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(oi => oi.ProductVariant)
            .WithMany(v => v.OrderItems)
            .HasForeignKey(oi => oi.ProductVariantId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Ignore(oi => oi.Product);

        builder.ToTable(table =>
        {
            table.HasCheckConstraint("CK_OrderItems_Quantity_Positive", "\"Quantity\" > 0");
            table.HasCheckConstraint("CK_OrderItems_Price_NonNegative", "\"PriceAtPurchase\" >= 0");
        });
    }
}
