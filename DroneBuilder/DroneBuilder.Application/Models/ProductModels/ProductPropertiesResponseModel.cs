using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Models.ProductModels;

public class ProductPropertiesResponseModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public ICollection<PropertyResponseModel> Properties { get; set; } = [];
}