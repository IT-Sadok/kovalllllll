namespace DroneBuilder.Application.Models.ProductModels;

public class ProductResponseModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
    public ICollection<PropertyResponseModel> Properties { get; set; } = [];
}