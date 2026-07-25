using DroneBuilder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DroneBuilder.Infrastructure.EntityConfigurations;

public class ProductVariantEntityConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Sku).IsRequired().HasMaxLength(100);
        builder.Property(v => v.Name).HasMaxLength(200);
        builder.Property(v => v.Price).IsRequired().HasColumnType("decimal(18,2)");
        builder.Property(v => v.CurrencyCode).IsRequired().HasMaxLength(3);
        builder.HasIndex(v => v.Sku).IsUnique();
        builder.HasIndex(v => new { v.ProductId, v.IsDefault })
            .HasFilter("\"IsDefault\" = TRUE")
            .IsUnique();
        builder.ToTable(table =>
            table.HasCheckConstraint("CK_ProductVariants_Price_NonNegative", "\"Price\" >= 0"));
    }
}
