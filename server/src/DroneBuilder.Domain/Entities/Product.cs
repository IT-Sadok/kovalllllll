using DroneBuilder.Domain.Entities.Components;

namespace DroneBuilder.Domain.Entities;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public ProductCategory Category { get; set; }
    public string? Manufacturer { get; set; }
    public decimal? WeightGrams { get; set; }
    public Guid? GroupId { get; set; }
    public ProductGroup? Group { get; set; }
    public string? VariantName { get; set; }
    public string? ExternalSource { get; set; }
    public string? ExternalId { get; set; }
    public string? SourceUrl { get; set; }
    public bool NeedsReview { get; set; }
    public bool IsDeleted { get; set; }

    public ICollection<Image> Images { get; set; } = [];
    public ICollection<ProductAttribute> Attributes { get; set; } = [];
    public ComponentSpec? Spec { get; set; }
}
