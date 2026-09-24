using DroneBuilder.Domain.Entities.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DroneBuilder.Infrastructure.EntityConfigurations;

public class FrameSpecEntityConfiguration : IEntityTypeConfiguration<FrameSpec>
{
    public void Configure(EntityTypeBuilder<FrameSpec> builder)
    {
        builder.Property(s => s.MaxPropSizeInch).HasPrecision(4, 1);
    }
}
