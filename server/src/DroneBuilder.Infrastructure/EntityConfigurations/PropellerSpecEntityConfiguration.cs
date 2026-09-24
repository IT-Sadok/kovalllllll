using DroneBuilder.Domain.Entities.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DroneBuilder.Infrastructure.EntityConfigurations;

public class PropellerSpecEntityConfiguration : IEntityTypeConfiguration<PropellerSpec>
{
    public void Configure(EntityTypeBuilder<PropellerSpec> builder)
    {
        builder.Property(s => s.DiameterInch).HasPrecision(4, 1);
        builder.Property(s => s.PitchInch).HasPrecision(4, 1);
        builder.Property(s => s.HubMm).HasPrecision(4, 1);
    }
}
