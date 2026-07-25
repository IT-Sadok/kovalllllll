namespace DroneBuilder.Domain.Entities;

public class ProductCategory : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public ProductCategory? Parent { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<ProductCategory> Children { get; set; } = [];
    public ICollection<Product> Products { get; set; } = [];
}
