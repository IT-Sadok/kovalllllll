using DroneBuilder.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DroneBuilder.Infrastructure.EntityConfigurations;

public class MessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Type).IsRequired().HasMaxLength(100);
        builder.Property(o => o.Payload).IsRequired();
        builder.HasIndex(o => o.ProcessedAt);
        builder.HasIndex(o => o.CreatedAt);
        builder.ToTable("Messages");
    }
}
