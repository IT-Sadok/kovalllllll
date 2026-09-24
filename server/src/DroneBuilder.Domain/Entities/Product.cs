using DroneBuilder.Domain.Entities.Components;

namespace DroneBuilder.Domain.Entities;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }

    public ICollection<Image> Images { get; set; } = [];
    public ICollection<ProductPropertyValue> ProductPropertyValues { get; set; } = [];
    public ComponentSpec? Spec { get; set; }
}
