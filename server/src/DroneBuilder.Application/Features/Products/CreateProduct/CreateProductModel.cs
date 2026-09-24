using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Features.Products.CreateProduct;

public class CreateProductModel
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public ProductCategory? Category { get; set; }
    public string? Manufacturer { get; set; }
    public decimal? WeightGrams { get; set; }
}
