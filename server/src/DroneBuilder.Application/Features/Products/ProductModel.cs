using DroneBuilder.Application.Features.Images;
namespace DroneBuilder.Application.Features.Products;

public class ProductModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Manufacturer { get; set; }
    public decimal? WeightGrams { get; set; }
    public ICollection<ProductAttributeModel> Attributes { get; set; } = [];
    public ICollection<ImageModel> Images { get; set; } = [];
    public int StockQuantity { get; set; }
    public ComponentSpecModel? Spec { get; set; }
}
