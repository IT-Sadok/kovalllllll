using DroneBuilder.Domain.Entities.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DroneBuilder.Infrastructure.EntityConfigurations;

public class AntennaSpecEntityConfiguration : IEntityTypeConfiguration<AntennaSpec>
{
    public void Configure(EntityTypeBuilder<AntennaSpec> builder)
    {
        builder.Property(s => s.Connector).HasColumnName(nameof(RfConnector));
    }
}
