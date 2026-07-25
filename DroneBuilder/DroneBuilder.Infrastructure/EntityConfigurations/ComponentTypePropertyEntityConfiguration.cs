using DroneBuilder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DroneBuilder.Infrastructure.EntityConfigurations;

public class ComponentTypePropertyEntityConfiguration : IEntityTypeConfiguration<ComponentTypeProperty>
{
    public void Configure(EntityTypeBuilder<ComponentTypeProperty> builder)
    {
        builder.HasKey(item => item.Id);
        builder.HasOne(item => item.ComponentType)
            .WithMany(type => type.Properties)
            .HasForeignKey(item => item.ComponentTypeId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(item => item.Property)
            .WithMany(property => property.ComponentTypes)
            .HasForeignKey(item => item.PropertyId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(item => new { item.ComponentTypeId, item.PropertyId })
            .IsUnique();
        builder.ToTable(table =>
            table.HasCheckConstraint(
                "CK_ComponentTypeProperties_SortOrder_NonNegative",
                "\"SortOrder\" >= 0"));
    }
}
