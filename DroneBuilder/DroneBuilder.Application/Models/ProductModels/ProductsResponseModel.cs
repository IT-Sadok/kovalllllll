namespace DroneBuilder.Application.Models.ProductModels;

public class ProductsResponseModel
{
    public IEnumerable<ProductResponseModel> Products { get; set; } = [];
}