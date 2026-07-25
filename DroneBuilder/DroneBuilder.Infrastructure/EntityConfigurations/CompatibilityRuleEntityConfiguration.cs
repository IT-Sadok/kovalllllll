using DroneBuilder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DroneBuilder.Infrastructure.EntityConfigurations;

public sealed class CompatibilityRuleEntityConfiguration : IEntityTypeConfiguration<CompatibilityRule>
{
    public void Configure(EntityTypeBuilder<CompatibilityRule> builder)
    {
        builder.HasKey(rule => rule.Id);
        builder.Property(rule => rule.Code).IsRequired().HasMaxLength(100);
        builder.Property(rule => rule.Name).IsRequired().HasMaxLength(200);
        builder.Property(rule => rule.Operator).IsRequired().HasConversion<int>();
        builder.Property(rule => rule.FailureMessage).HasMaxLength(1000);
        builder.HasIndex(rule => rule.Code).IsUnique();
        builder.HasIndex(rule => new
        {
            rule.LeftComponentTypeId,
            rule.RightComponentTypeId,
            rule.IsActive
        });

        builder.HasOne(rule => rule.LeftComponentType)
            .WithMany()
            .HasForeignKey(rule => rule.LeftComponentTypeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(rule => rule.RightComponentType)
            .WithMany()
            .HasForeignKey(rule => rule.RightComponentTypeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(rule => rule.LeftProperty)
            .WithMany()
            .HasForeignKey(rule => rule.LeftPropertyId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(rule => rule.RightProperty)
            .WithMany()
            .HasForeignKey(rule => rule.RightPropertyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
