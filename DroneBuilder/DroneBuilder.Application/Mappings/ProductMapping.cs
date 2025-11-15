using Mapster;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Application.Models.ProductModels;

namespace DroneBuilder.Application.Mappings;

public class ProductMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Product, ProductResponseModel>();
        config.NewConfig<ICollection<Product>, ProductsResponseModel>()
            .Map(dest => dest.Products, src => src);
        config.NewConfig<Product, ProductPropertiesResponseModel>();
    }
}