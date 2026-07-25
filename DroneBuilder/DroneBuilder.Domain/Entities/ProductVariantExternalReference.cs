namespace DroneBuilder.Domain.Entities;

public class ProductVariantExternalReference : AuditableEntity
{
    public Guid ImportSourceId { get; set; }
    public ImportSource? ImportSource { get; set; }
    public Guid ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string? SourceUrl { get; set; }
    public string? ContentHash { get; set; }
    public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;
}
