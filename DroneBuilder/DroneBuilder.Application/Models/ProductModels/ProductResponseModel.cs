using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Models.ProductModels;

public class ProductResponseModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public ICollection<Image> Images { get; set; } = [];
    public ICollection<Property> Properties { get; set; } = [];
}