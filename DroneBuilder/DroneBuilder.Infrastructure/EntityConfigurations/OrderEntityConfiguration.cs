using DroneBuilder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DroneBuilder.Infrastructure.EntityConfigurations;

public class OrderEntityConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Status)
            .IsRequired();

        builder.Property(o => o.TotalPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(o => o.CurrencyCode)
            .IsRequired()
            .HasMaxLength(3);

        builder.Ignore(o => o.ShippingDetails);

        builder.OwnsOne(o => o.ShippingAddress, address =>
        {
            address.Property(a => a.FullName).IsRequired().HasMaxLength(200);
            address.Property(a => a.AddressLine1).IsRequired().HasMaxLength(300);
            address.Property(a => a.AddressLine2).HasMaxLength(300);
            address.Property(a => a.City).IsRequired().HasMaxLength(100);
            address.Property(a => a.State).HasMaxLength(100);
            address.Property(a => a.PostalCode).IsRequired().HasMaxLength(30);
            address.Property(a => a.Country).IsRequired().HasMaxLength(100);
            address.Property(a => a.PhoneNumber).IsRequired().HasMaxLength(50);
        });

        builder.HasMany(o => o.OrderItems)
            .WithOne(oi => oi.Order!)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(o => new { o.UserId, o.CreatedAt });

        builder.ToTable(table =>
            table.HasCheckConstraint("CK_Orders_TotalPrice_NonNegative", "\"TotalPrice\" >= 0"));
    }
}
