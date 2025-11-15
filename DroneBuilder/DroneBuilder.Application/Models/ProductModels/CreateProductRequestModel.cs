using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Models.ProductModels;

public class CreateProductRequestModel
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public ICollection<Image> Images { get; set; } = [];
    public ICollection<Property> Properties { get; set; } = [];
}