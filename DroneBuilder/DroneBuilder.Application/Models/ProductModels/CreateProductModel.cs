namespace DroneBuilder.Application.Models.ProductModels;

public class CreateProductModel
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
    public CreatePropertyModel Properties { get; set; }
}