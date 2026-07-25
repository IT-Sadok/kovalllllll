namespace DroneBuilder.Domain.Entities;

public class ImportBatch : AuditableEntity
{
    public Guid ImportSourceId { get; set; }
    public ImportSource? ImportSource { get; set; }
    public ImportStatus Status { get; set; } = ImportStatus.Pending;
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public int TotalItems { get; set; }
    public int ProcessedItems { get; set; }
    public int FailedItems { get; set; }
    public ICollection<ImportItem> Items { get; set; } = [];
}
