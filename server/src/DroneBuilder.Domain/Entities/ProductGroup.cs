namespace DroneBuilder.Domain.Entities;

public class ProductGroup
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? ExternalSource { get; set; }
    public string? ExternalId { get; set; }

    public ICollection<Product> Products { get; set; } = [];
}
