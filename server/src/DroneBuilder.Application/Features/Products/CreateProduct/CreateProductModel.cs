namespace DroneBuilder.Application.Features.Products.CreateProduct;

public class CreateProductModel
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
}
