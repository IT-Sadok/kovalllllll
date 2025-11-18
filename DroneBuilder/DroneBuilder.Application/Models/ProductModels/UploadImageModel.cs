namespace DroneBuilder.Application.Models.ProductModels;

public class UploadImageModel
{
    public string Url { get; set; }
    public string FileName { get; set; }
    public Guid ProductId { get; set; }
    public DateTime UploadedAt { get; set; }
}