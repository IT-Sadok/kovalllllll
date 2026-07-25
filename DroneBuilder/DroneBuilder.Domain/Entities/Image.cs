namespace DroneBuilder.Domain.Entities;

public class Image : AuditableEntity
{
    public string Url { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
}
