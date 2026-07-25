using DroneBuilder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DroneBuilder.Infrastructure.EntityConfigurations;

public class CartItemEntityConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.HasKey(ci => ci.Id);

        builder.Property(ci => ci.Quantity)
            .IsRequired();

        builder.HasOne(ci => ci.Cart)
            .WithMany(c => c.CartItems)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ci => ci.ProductVariant)
            .WithMany(v => v.CartItems)
            .HasForeignKey(ci => ci.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(ci => ci.ProductId);
        builder.Ignore(ci => ci.Product);
        builder.Ignore(ci => ci.ProductName);

        builder.HasIndex(ci => new { ci.CartId, ci.ProductVariantId })
            .IsUnique();

        builder.ToTable(table =>
            table.HasCheckConstraint("CK_CartItems_Quantity_Positive", "\"Quantity\" > 0"));
    }
}
