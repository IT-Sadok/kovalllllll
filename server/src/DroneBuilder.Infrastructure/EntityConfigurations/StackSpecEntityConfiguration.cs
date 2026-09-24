using DroneBuilder.Domain.Entities.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DroneBuilder.Infrastructure.EntityConfigurations;

public class StackSpecEntityConfiguration : IEntityTypeConfiguration<StackSpec>
{
    public void Configure(EntityTypeBuilder<StackSpec> builder)
    {
        builder.Property(s => s.MountPattern).HasColumnName(nameof(StackSpec.MountPattern));
        builder.Property(s => s.MinCells).HasColumnName(nameof(StackSpec.MinCells));
        builder.Property(s => s.MaxCells).HasColumnName(nameof(StackSpec.MaxCells));
        builder.Property(s => s.ContinuousCurrentA).HasColumnName(nameof(StackSpec.ContinuousCurrentA)).HasPrecision(5, 1);
        builder.Property(s => s.BatteryConnector).HasColumnName(nameof(StackSpec.BatteryConnector));
    }
}
