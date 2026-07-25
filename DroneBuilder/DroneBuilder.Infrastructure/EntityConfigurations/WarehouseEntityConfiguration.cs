using DroneBuilder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DroneBuilder.Infrastructure.EntityConfigurations;

public class WarehouseEntityConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Code)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(w => w.CreatedAt)
            .IsRequired();

        builder.HasIndex(w => w.Code)
            .IsUnique();

        builder.HasMany(w => w.WarehouseItems)
            .WithOne(wi => wi.Warehouse)
            .HasForeignKey(wi => wi.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
