using DroneBuilder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DroneBuilder.Infrastructure.EntityConfigurations;

public class ValueEntityConfiguration : IEntityTypeConfiguration<Value>
{
    public void Configure(EntityTypeBuilder<Value> builder)
    {
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Text)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.Code)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.NumericValue)
            .HasColumnType("decimal(18,6)");

        builder.HasIndex(v => v.Code)
            .IsUnique();

        builder.HasMany(v => v.Properties)
            .WithMany(p => p.Values);
    }
}
