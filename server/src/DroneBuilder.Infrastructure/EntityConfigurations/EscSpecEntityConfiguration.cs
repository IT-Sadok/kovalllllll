using DroneBuilder.Domain.Entities.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DroneBuilder.Infrastructure.EntityConfigurations;

public class EscSpecEntityConfiguration : IEntityTypeConfiguration<EscSpec>
{
    public void Configure(EntityTypeBuilder<EscSpec> builder)
    {
        builder.Property(s => s.MountPattern).HasColumnName(nameof(EscSpec.MountPattern));
        builder.Property(s => s.MinCells).HasColumnName(nameof(EscSpec.MinCells));
        builder.Property(s => s.MaxCells).HasColumnName(nameof(EscSpec.MaxCells));
        builder.Property(s => s.ContinuousCurrentA).HasColumnName(nameof(EscSpec.ContinuousCurrentA)).HasPrecision(5, 1);
        builder.Property(s => s.BatteryConnector).HasColumnName(nameof(EscSpec.BatteryConnector));
    }
}
