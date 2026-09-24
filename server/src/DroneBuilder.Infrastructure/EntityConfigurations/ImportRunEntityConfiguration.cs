using DroneBuilder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DroneBuilder.Infrastructure.EntityConfigurations;

public class ImportRunEntityConfiguration : IEntityTypeConfiguration<ImportRun>
{
    public void Configure(EntityTypeBuilder<ImportRun> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Source)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.Error)
            .HasMaxLength(2000);

        builder.HasIndex(r => r.CreatedAt);
    }
}
