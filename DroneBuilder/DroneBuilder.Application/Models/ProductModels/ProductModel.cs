namespace DroneBuilder.Application.Models.ProductModels;

public class ProductModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
    public ICollection<PropertyModel> Properties { get; set; } = [];
}