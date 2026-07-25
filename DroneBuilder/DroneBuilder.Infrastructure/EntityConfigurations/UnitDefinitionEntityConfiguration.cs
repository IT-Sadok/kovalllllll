using DroneBuilder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DroneBuilder.Infrastructure.EntityConfigurations;

public class UnitDefinitionEntityConfiguration : IEntityTypeConfiguration<UnitDefinition>
{
    public void Configure(EntityTypeBuilder<UnitDefinition> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Code).IsRequired().HasMaxLength(50);
        builder.Property(u => u.Name).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Symbol).IsRequired().HasMaxLength(30);
        builder.Property(u => u.Dimension).HasMaxLength(100);
        builder.Property(u => u.ConversionFactorToBase)
            .HasColumnType("decimal(18,8)");
        builder.HasIndex(u => u.Code).IsUnique();
        builder.ToTable(table =>
            table.HasCheckConstraint(
                "CK_UnitDefinitions_ConversionFactor_Positive",
                "\"ConversionFactorToBase\" > 0"));
    }
}
