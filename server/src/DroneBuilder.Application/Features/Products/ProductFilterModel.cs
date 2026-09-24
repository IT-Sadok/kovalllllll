using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Features.Products;

public class ProductFilterModel
{
    public string? Name { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public ProductCategory? Category { get; set; }
    public bool? CollapseVariants { get; set; }
}
