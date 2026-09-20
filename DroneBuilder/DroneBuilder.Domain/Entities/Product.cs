namespace DroneBuilder.Domain.Entities;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    /// <summary>
    /// Products are delisted rather than deleted: order history references them, and the name and
    /// price captured on each OrderItem do not cover the images a past order still shows.
    /// </summary>
    public bool IsDeleted { get; set; }

    public ICollection<Image> Images { get; set; } = [];
    public ICollection<ProductPropertyValue> ProductPropertyValues { get; set; } = [];
}
