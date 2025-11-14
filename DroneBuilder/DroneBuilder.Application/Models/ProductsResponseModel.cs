namespace DroneBuilder.Application.Models;

public class ProductsResponseModel
{
    public IEnumerable<ProductResponseModel> Products { get; set; } = [];
}