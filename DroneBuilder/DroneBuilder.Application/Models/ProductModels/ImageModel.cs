namespace DroneBuilder.Application.Models.ProductModels;

public class ImageModel
{
    public Guid Id { get; set; }
    public string Url { get; set; }
    public string FileName { get; set; }
    public DateTime UploadedAt { get; set; }
}