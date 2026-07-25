using DroneBuilder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DroneBuilder.Infrastructure.EntityConfigurations;

public class UnitAliasEntityConfiguration : IEntityTypeConfiguration<UnitAlias>
{
    public void Configure(EntityTypeBuilder<UnitAlias> builder)
    {
        builder.HasKey(alias => alias.Id);
        builder.Property(alias => alias.Alias).IsRequired().HasMaxLength(100);
        builder.Property(alias => alias.NormalizedAlias).IsRequired().HasMaxLength(100);
        builder.HasOne(alias => alias.UnitDefinition)
            .WithMany(unit => unit.Aliases)
            .HasForeignKey(alias => alias.UnitDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(alias => alias.NormalizedAlias).IsUnique();
    }
}

public class PropertyAliasEntityConfiguration : IEntityTypeConfiguration<PropertyAlias>
{
    public void Configure(EntityTypeBuilder<PropertyAlias> builder)
    {
        builder.HasKey(alias => alias.Id);
        ConfigureAlias(builder);
        builder.HasOne(alias => alias.Property)
            .WithMany(property => property.Aliases)
            .HasForeignKey(alias => alias.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(alias => alias.ImportSource)
            .WithMany(source => source.PropertyAliases)
            .HasForeignKey(alias => alias.ImportSourceId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(alias => alias.NormalizedAlias)
            .HasFilter("\"ImportSourceId\" IS NULL")
            .IsUnique();
        builder.HasIndex(alias => new { alias.ImportSourceId, alias.NormalizedAlias })
            .HasFilter("\"ImportSourceId\" IS NOT NULL")
            .IsUnique();
    }

    private static void ConfigureAlias(EntityTypeBuilder<PropertyAlias> builder)
    {
        builder.Property(alias => alias.Alias).IsRequired().HasMaxLength(200);
        builder.Property(alias => alias.NormalizedAlias).IsRequired().HasMaxLength(200);
    }
}

public class ValueAliasEntityConfiguration : IEntityTypeConfiguration<ValueAlias>
{
    public void Configure(EntityTypeBuilder<ValueAlias> builder)
    {
        builder.HasKey(alias => alias.Id);
        builder.Property(alias => alias.Alias).IsRequired().HasMaxLength(200);
        builder.Property(alias => alias.NormalizedAlias).IsRequired().HasMaxLength(200);
        builder.HasOne(alias => alias.Value)
            .WithMany(value => value.Aliases)
            .HasForeignKey(alias => alias.ValueId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(alias => alias.ImportSource)
            .WithMany(source => source.ValueAliases)
            .HasForeignKey(alias => alias.ImportSourceId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(alias => alias.NormalizedAlias)
            .HasFilter("\"ImportSourceId\" IS NULL")
            .IsUnique();
        builder.HasIndex(alias => new { alias.ImportSourceId, alias.NormalizedAlias })
            .HasFilter("\"ImportSourceId\" IS NOT NULL")
            .IsUnique();
    }
}
