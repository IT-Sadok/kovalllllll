using DroneBuilder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DroneBuilder.Infrastructure.EntityConfigurations;

public class ValueEntityConfiguration : IEntityTypeConfiguration<Value>
{
    public void Configure(EntityTypeBuilder<Value> builder)
    {
        builder.HasKey(v => v.Id);

        builder.Property(v => v.ValueText)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasMany(v => v.Properties)
            .WithMany(p => p.Values)
            .UsingEntity<Dictionary<string, object>>(
                "PropertyValue",
                j => j
                    .HasOne<Property>()
                    .WithMany()
                    .HasForeignKey("PropertyId")
                    .OnDelete(DeleteBehavior.Cascade),
                j => j
                    .HasOne<Value>()
                    .WithMany()
                    .HasForeignKey("ValueId")
                    .OnDelete(DeleteBehavior.Cascade),
                j => { j.HasKey("PropertyId", "ValueId"); });
    }
}