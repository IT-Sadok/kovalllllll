using DroneBuilder.Domain.Entities.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DroneBuilder.Infrastructure.EntityConfigurations;

public class RadioSpecEntityConfiguration : IEntityTypeConfiguration<RadioSpec>
{
    public void Configure(EntityTypeBuilder<RadioSpec> builder)
    {
        builder.Property(s => s.Protocol).HasColumnName(nameof(RadioSpec.Protocol));
    }
}
