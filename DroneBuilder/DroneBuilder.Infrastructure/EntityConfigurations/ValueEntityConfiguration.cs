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

        builder.HasMany(v => v.Properties)
            .WithMany(p => p.Values);
    }
}