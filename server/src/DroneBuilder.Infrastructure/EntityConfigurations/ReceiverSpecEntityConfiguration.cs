using DroneBuilder.Domain.Entities.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DroneBuilder.Infrastructure.EntityConfigurations;

public class ReceiverSpecEntityConfiguration : IEntityTypeConfiguration<ReceiverSpec>
{
    public void Configure(EntityTypeBuilder<ReceiverSpec> builder)
    {
        builder.Property(s => s.Protocol).HasColumnName(nameof(ReceiverSpec.Protocol));
    }
}
