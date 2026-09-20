namespace DroneBuilder.Application.Models.ProductModels;

public class UploadImageModel
{
    public string Url { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public DateTime UploadedAt { get; set; }
}
