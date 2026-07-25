using DroneBuilder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DroneBuilder.Infrastructure.EntityConfigurations;

public class ProductVariantPropertyValueEntityConfiguration
    : IEntityTypeConfiguration<ProductVariantPropertyValue>
{
    public void Configure(EntityTypeBuilder<ProductVariantPropertyValue> builder)
    {
        builder.HasKey(item => item.Id);
        ConfigureValueColumns(builder);

        builder.HasOne(item => item.ProductVariant)
            .WithMany(variant => variant.Specifications)
            .HasForeignKey(item => item.ProductVariantId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(item => item.Property)
            .WithMany(property => property.ProductVariantPropertyValues)
            .HasForeignKey(item => item.PropertyId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(item => item.Value)
            .WithMany(value => value.ProductVariantPropertyValues)
            .HasForeignKey(item => item.ValueId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PropertyValue>()
            .WithMany()
            .HasForeignKey(item => new { item.PropertyId, item.ValueId })
            .HasPrincipalKey(link => new { link.PropertyId, link.ValueId })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(item => new { item.ProductVariantId, item.PropertyId });
        AddValueConstraints(builder);
    }

    private static void ConfigureValueColumns(EntityTypeBuilder<ProductVariantPropertyValue> builder)
    {
        builder.Property(item => item.TextValue).HasMaxLength(1000);
        builder.Property(item => item.NumericValue).HasColumnType("decimal(18,6)");
        builder.Property(item => item.MinNumericValue).HasColumnType("decimal(18,6)");
        builder.Property(item => item.MaxNumericValue).HasColumnType("decimal(18,6)");
    }

    private static void AddValueConstraints(EntityTypeBuilder<ProductVariantPropertyValue> builder)
    {
        builder.ToTable(table =>
        {
            table.HasCheckConstraint(
                "CK_ProductVariantPropertyValues_ExactlyOneValue",
                ValueConstraintSql.ExactlyOneRepresentation);
            table.HasCheckConstraint(
                "CK_ProductVariantPropertyValues_CompleteRange",
                ValueConstraintSql.CompleteRange);
            table.HasCheckConstraint(
                "CK_ProductVariantPropertyValues_ValidRange",
                ValueConstraintSql.ValidRange);
        });
    }
}
