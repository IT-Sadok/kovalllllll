using DroneBuilder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DroneBuilder.Infrastructure.EntityConfigurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Type).IsRequired().HasMaxLength(100);
        builder.Property(o => o.Payload).IsRequired();
        builder.HasIndex(o => o.ProcessedAt);
        builder.HasIndex(o => o.CreatedAt);
    }
}