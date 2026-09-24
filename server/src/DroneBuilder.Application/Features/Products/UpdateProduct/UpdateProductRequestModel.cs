namespace DroneBuilder.Application.Features.Products.UpdateProduct;

public class UpdateProductRequestModel
{
    public string? Name { get; set; }
    public decimal? Price { get; set; }
    public string? Category { get; set; }
    public string? Manufacturer { get; set; }
    public decimal? WeightGrams { get; set; }
}
