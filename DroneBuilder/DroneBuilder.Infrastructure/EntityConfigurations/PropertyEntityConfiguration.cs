using DroneBuilder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DroneBuilder.Infrastructure.EntityConfigurations;

public class PropertyEntityConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Code)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.DataType)
            .IsRequired()
            .HasConversion<int>();

        builder.HasIndex(p => p.Code)
            .IsUnique();

        builder.HasOne(p => p.UnitDefinition)
            .WithMany(u => u.Properties)
            .HasForeignKey(p => p.UnitDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Values)
            .WithMany(v => v.Properties)
            .UsingEntity<PropertyValue>(
                right => right
                    .HasOne<Value>()
                    .WithMany()
                    .HasForeignKey(item => item.ValueId)
                    .OnDelete(DeleteBehavior.Cascade),
                left => left
                    .HasOne<Property>()
                    .WithMany()
                    .HasForeignKey(item => item.PropertyId)
                    .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.ToTable("PropertyValues");
                    join.HasKey(item => new { item.PropertyId, item.ValueId });
                });
    }
}
