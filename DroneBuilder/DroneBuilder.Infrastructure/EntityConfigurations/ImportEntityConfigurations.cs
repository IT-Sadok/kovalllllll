using DroneBuilder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DroneBuilder.Infrastructure.EntityConfigurations;

public class ImportSourceEntityConfiguration : IEntityTypeConfiguration<ImportSource>
{
    public void Configure(EntityTypeBuilder<ImportSource> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Code).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(200);
        builder.Property(s => s.BaseUrl).HasMaxLength(1000);
        builder.HasIndex(s => s.Code).IsUnique();
    }
}

public class ImportBatchEntityConfiguration : IEntityTypeConfiguration<ImportBatch>
{
    public void Configure(EntityTypeBuilder<ImportBatch> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Status).HasConversion<int>();
        builder.HasOne(b => b.ImportSource)
            .WithMany(s => s.Batches)
            .HasForeignKey(b => b.ImportSourceId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(b => new { b.ImportSourceId, b.CreatedAt });
    }
}

public class ImportItemEntityConfiguration : IEntityTypeConfiguration<ImportItem>
{
    public void Configure(EntityTypeBuilder<ImportItem> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.ExternalId).IsRequired().HasMaxLength(300);
        builder.Property(i => i.RawPayload).IsRequired().HasColumnType("jsonb");
        builder.Property(i => i.ContentHash).HasMaxLength(128);
        builder.Property(i => i.Status).HasConversion<int>();
        builder.Property(i => i.Error).HasMaxLength(4000);
        builder.HasOne(i => i.ImportBatch)
            .WithMany(b => b.Items)
            .HasForeignKey(i => i.ImportBatchId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(i => new { i.ImportBatchId, i.ExternalId }).IsUnique();
        builder.HasIndex(i => i.Status);
    }
}

public class ProductExternalReferenceEntityConfiguration : IEntityTypeConfiguration<ProductExternalReference>
{
    public void Configure(EntityTypeBuilder<ProductExternalReference> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.ExternalId).IsRequired().HasMaxLength(300);
        builder.Property(r => r.SourceUrl).HasMaxLength(1000);
        builder.Property(r => r.ContentHash).HasMaxLength(128);
        builder.HasOne(r => r.ImportSource)
            .WithMany(s => s.ProductReferences)
            .HasForeignKey(r => r.ImportSourceId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(r => r.Product)
            .WithMany(p => p.ExternalReferences)
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(r => new { r.ImportSourceId, r.ExternalId }).IsUnique();
    }
}

public class ProductVariantExternalReferenceEntityConfiguration
    : IEntityTypeConfiguration<ProductVariantExternalReference>
{
    public void Configure(EntityTypeBuilder<ProductVariantExternalReference> builder)
    {
        builder.HasKey(reference => reference.Id);
        builder.Property(reference => reference.ExternalId).IsRequired().HasMaxLength(300);
        builder.Property(reference => reference.SourceUrl).HasMaxLength(1000);
        builder.Property(reference => reference.ContentHash).HasMaxLength(128);
        builder.HasOne(reference => reference.ImportSource)
            .WithMany(source => source.ProductVariantReferences)
            .HasForeignKey(reference => reference.ImportSourceId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(reference => reference.ProductVariant)
            .WithMany(variant => variant.ExternalReferences)
            .HasForeignKey(reference => reference.ProductVariantId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(reference => new { reference.ImportSourceId, reference.ExternalId })
            .IsUnique();
    }
}
