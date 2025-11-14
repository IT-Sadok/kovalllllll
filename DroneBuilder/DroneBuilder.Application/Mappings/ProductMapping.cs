using Mapster;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Application.Models;

namespace DroneBuilder.Application.Mappings;

public class ProductMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Product, ProductResponseModel>();
        config.NewConfig<ProductResponseModel, ProductsResponseModel>();
        config.NewConfig<Product, ProductPropertiesResponseModel>();
    }
}