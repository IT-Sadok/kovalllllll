namespace DroneBuilder.Domain.Entities;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public decimal Price { get; set; }
    public ICollection<Image> Images = new List<Image>();
    public ICollection<Property> Properties = new List<Property>();
}