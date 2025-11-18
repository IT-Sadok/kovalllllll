namespace DroneBuilder.Application.Models.ProductModels;

public class ProductImagesResponseModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public ICollection<ImageResponseModel> Images { get; set; } = [];
}