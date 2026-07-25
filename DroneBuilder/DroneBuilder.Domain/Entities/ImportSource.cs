namespace DroneBuilder.Domain.Entities;

public class ImportSource : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? BaseUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<ImportBatch> Batches { get; set; } = [];
    public ICollection<ProductExternalReference> ProductReferences { get; set; } = [];
    public ICollection<ProductVariantExternalReference> ProductVariantReferences { get; set; } = [];
    public ICollection<PropertyAlias> PropertyAliases { get; set; } = [];
    public ICollection<ValueAlias> ValueAliases { get; set; } = [];
}
