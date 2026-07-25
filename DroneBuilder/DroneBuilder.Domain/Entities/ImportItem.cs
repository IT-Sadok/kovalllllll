namespace DroneBuilder.Domain.Entities;

public class ImportItem : AuditableEntity
{
    public Guid ImportBatchId { get; set; }
    public ImportBatch? ImportBatch { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string RawPayload { get; set; } = "{}";
    public string? ContentHash { get; set; }
    public ImportStatus Status { get; set; } = ImportStatus.Pending;
    public string? Error { get; set; }
    public Guid? ProductId { get; set; }
    public Product? Product { get; set; }
}
