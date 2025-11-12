namespace DroneBuilder.Domain.Entities;

public class Image
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Url { get; set; }
    public string FileName { get; set; }
    public DateTime UploadedAt { get; set; }
    public Guid ProductId { get; set; }
    public Product Product { get; set; }
}