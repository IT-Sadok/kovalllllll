using DroneBuilder.Domain.Entities.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DroneBuilder.Infrastructure.EntityConfigurations;

public class FlightControllerSpecEntityConfiguration : IEntityTypeConfiguration<FlightControllerSpec>
{
    public void Configure(EntityTypeBuilder<FlightControllerSpec> builder)
    {
        builder.Property(s => s.MountPattern).HasColumnName(nameof(FlightControllerSpec.MountPattern));
        builder.Property(s => s.MinCells).HasColumnName(nameof(FlightControllerSpec.MinCells));
        builder.Property(s => s.MaxCells).HasColumnName(nameof(FlightControllerSpec.MaxCells));
    }
}
