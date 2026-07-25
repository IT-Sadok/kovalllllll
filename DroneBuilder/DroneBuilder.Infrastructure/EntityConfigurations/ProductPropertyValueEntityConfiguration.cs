using DroneBuilder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DroneBuilder.Infrastructure.EntityConfigurations;

public class ProductPropertyValueEntityConfiguration : IEntityTypeConfiguration<ProductPropertyValue>
{
    public void Configure(EntityTypeBuilder<ProductPropertyValue> builder)
    {
        builder.HasKey(v => v.Id);
        builder.Property(v => v.TextValue).HasMaxLength(1000);
        builder.Property(v => v.NumericValue).HasColumnType("decimal(18,6)");
        builder.Property(v => v.MinNumericValue).HasColumnType("decimal(18,6)");
        builder.Property(v => v.MaxNumericValue).HasColumnType("decimal(18,6)");

        builder.HasOne(v => v.Product)
            .WithMany(p => p.ProductPropertyValues)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.Property)
            .WithMany(p => p.ProductPropertyValues)
            .HasForeignKey(v => v.PropertyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.Value)
            .WithMany(p => p.ProductPropertyValues)
            .HasForeignKey(v => v.ValueId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<PropertyValue>()
            .WithMany()
            .HasForeignKey(v => new { v.PropertyId, v.ValueId })
            .HasPrincipalKey(link => new { link.PropertyId, link.ValueId })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(v => new { v.ProductId, v.PropertyId });
        builder.ToTable(table =>
        {
            table.HasCheckConstraint(
                "CK_ProductPropertyValues_ExactlyOneValue",
                ValueConstraintSql.ExactlyOneRepresentation);
            table.HasCheckConstraint(
                "CK_ProductPropertyValues_CompleteRange",
                ValueConstraintSql.CompleteRange);
            table.HasCheckConstraint(
                "CK_ProductPropertyValues_ValidRange",
                ValueConstraintSql.ValidRange);
        });
    }
}
