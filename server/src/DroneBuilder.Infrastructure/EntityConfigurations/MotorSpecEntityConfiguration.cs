using DroneBuilder.Domain.Entities.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DroneBuilder.Infrastructure.EntityConfigurations;

public class MotorSpecEntityConfiguration : IEntityTypeConfiguration<MotorSpec>
{
    public void Configure(EntityTypeBuilder<MotorSpec> builder)
    {
        builder.Property(s => s.StatorSize).HasMaxLength(10);
        builder.Property(s => s.MountPattern).HasColumnName(nameof(MotorSpec.MountPattern));
        builder.Property(s => s.MinCells).HasColumnName(nameof(MotorSpec.MinCells));
        builder.Property(s => s.MaxCells).HasColumnName(nameof(MotorSpec.MaxCells));
        builder.Property(s => s.MaxCurrentA).HasPrecision(5, 1);
        builder.Property(s => s.ShaftMm).HasPrecision(4, 1);
    }
}
