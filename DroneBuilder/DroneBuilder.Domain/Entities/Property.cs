namespace DroneBuilder.Domain.Entities;

public class Property
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public ICollection<Value> Values { get; set; } = [];
    public ICollection<Product> Products { get; set; } = [];
}