namespace DroneBuilder.Application.Features.Images;

public class ImageModel
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public bool IsPrimary { get; set; }
}
