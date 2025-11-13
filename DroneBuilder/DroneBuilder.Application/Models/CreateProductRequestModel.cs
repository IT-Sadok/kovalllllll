using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Models;

public class CreateProductRequestModel
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public ICollection<Image> Images => new List<Image>();
    public ICollection<Property> Properties => new List<Property>();
}