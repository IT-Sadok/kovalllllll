namespace DroneBuilder.Application.Models.ProductModels;

public class UpdateProductRequestModel
{
    public string? Name { get; set; }
    public decimal? Price { get; set; }
    public string? Category { get; set; }
}