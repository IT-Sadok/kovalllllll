namespace DroneBuilder.Application.Features.Products;

public class ProductGroupModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int VariantCount { get; set; }
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public int TotalStock { get; set; }
    public List<ProductVariantModel>? Variants { get; set; }
}

public record ProductVariantModel(Guid Id, string? VariantName, decimal Price, int StockQuantity);

public record ProductGroupSummary(Guid Id, string Name, int VariantCount, decimal MinPrice, decimal MaxPrice, int TotalStock);

public static class ProductGroupMappingExtensions
{
    public static ProductGroupModel ToModel(this ProductGroupSummary summary) => new()
    {
        Id = summary.Id,
        Name = summary.Name,
        VariantCount = summary.VariantCount,
        MinPrice = summary.MinPrice,
        MaxPrice = summary.MaxPrice,
        TotalStock = summary.TotalStock
    };
}
