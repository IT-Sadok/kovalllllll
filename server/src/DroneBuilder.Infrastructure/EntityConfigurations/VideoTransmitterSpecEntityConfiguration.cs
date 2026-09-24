using DroneBuilder.Domain.Entities.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DroneBuilder.Infrastructure.EntityConfigurations;

public class VideoTransmitterSpecEntityConfiguration : IEntityTypeConfiguration<VideoTransmitterSpec>
{
    public void Configure(EntityTypeBuilder<VideoTransmitterSpec> builder)
    {
        builder.Property(s => s.VideoSystem).HasColumnName(nameof(VideoTransmitterSpec.VideoSystem));
        builder.Property(s => s.AntennaConnector).HasColumnName(nameof(RfConnector));
        builder.Property(s => s.MountPattern).HasColumnName(nameof(VideoTransmitterSpec.MountPattern));
    }
}
