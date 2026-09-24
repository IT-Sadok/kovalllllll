using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Features.Products.UpdateProduct;

public class UpdateProductRequestModel
{
    public string? Name { get; set; }
    public decimal? Price { get; set; }
    public ProductCategory? Category { get; set; }
    public string? Manufacturer { get; set; }
    public decimal? WeightGrams { get; set; }
}
