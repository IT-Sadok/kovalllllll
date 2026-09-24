using DroneBuilder.Domain.Entities.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DroneBuilder.Infrastructure.EntityConfigurations;

public class BatterySpecEntityConfiguration : IEntityTypeConfiguration<BatterySpec>
{
    public void Configure(EntityTypeBuilder<BatterySpec> builder)
    {
        builder.Property(s => s.Connector).HasColumnName(nameof(EscSpec.BatteryConnector));
    }
}
