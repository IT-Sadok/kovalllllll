namespace DroneBuilder.Domain.Entities;

public class Image
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Url { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public bool IsPrimary { get; set; }
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
}
