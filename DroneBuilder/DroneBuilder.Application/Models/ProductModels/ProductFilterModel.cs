namespace DroneBuilder.Application.Models.ProductModels;

public class ProductFilterModel
{
    public string? Name { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Category { get; set; }
}