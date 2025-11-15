namespace DroneBuilder.Application.Models.ProductModels;

public class ProductsResponseModel
{
    public ICollection<ProductResponseModel> Products { get; set; } = [];
}