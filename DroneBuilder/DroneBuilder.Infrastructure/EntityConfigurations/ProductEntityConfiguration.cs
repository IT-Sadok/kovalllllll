using DroneBuilder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DroneBuilder.Infrastructure.EntityConfigurations;

public class ProductEntityConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(4000);

        builder.Property(p => p.Kind)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.IsActive)
            .IsRequired();

        builder.Property(p => p.PublicationStatus)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(ProductPublicationStatus.Published);

        builder.Ignore(p => p.Price);
        builder.Ignore(p => p.Category);

        builder.HasOne(p => p.ProductCategory)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.ProductCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.ComponentType)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.ComponentTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Variants)
            .WithOne(v => v.Product)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Images)
            .WithOne(i => i.Product)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => new { p.ProductCategoryId, p.IsActive, p.PublicationStatus });
        builder.HasIndex(p => p.ComponentTypeId);
    }
}
