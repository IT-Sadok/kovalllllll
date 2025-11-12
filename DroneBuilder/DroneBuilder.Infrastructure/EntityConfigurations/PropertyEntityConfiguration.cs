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

        builder.HasMany(p => p.Values)
            .WithMany(v => v.Properties)
            .UsingEntity<Dictionary<string, object>>(
                "PropertyValue",
                j => j
                    .HasOne<Value>()
                    .WithMany()
                    .HasForeignKey("ValueId")
                    .OnDelete(DeleteBehavior.Cascade),
                j => j
                    .HasOne<Property>()
                    .WithMany()
                    .HasForeignKey("PropertyId")
                    .OnDelete(DeleteBehavior.Cascade),
                j => { j.HasKey("PropertyId", "ValueId"); });
    }
}