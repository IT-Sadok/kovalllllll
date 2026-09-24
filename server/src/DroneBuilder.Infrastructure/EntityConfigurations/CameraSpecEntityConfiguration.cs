using DroneBuilder.Domain.Entities.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DroneBuilder.Infrastructure.EntityConfigurations;

public class CameraSpecEntityConfiguration : IEntityTypeConfiguration<CameraSpec>
{
    public void Configure(EntityTypeBuilder<CameraSpec> builder)
    {
        builder.Property(s => s.VideoSystem).HasColumnName(nameof(CameraSpec.VideoSystem));
    }
}
