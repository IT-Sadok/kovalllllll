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
        config.NewConfig<Product, ProductPropertiesResponseModel>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Category, src => src.Category)
            .Map(dest => dest.Price, src => src.Price)
            .Map(dest => dest.Properties, src => src.Properties);

        config.NewConfig<CreateProductModel, Product>()
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Price, src => src.Price)
            .Map(dest => dest.Category, src => src.Category)
            .Ignore(dest => dest.Properties)
            .Ignore(dest => dest.Images)
            .Ignore(dest => dest.Id);

        config.NewConfig<UpdateProductRequestModel, Product>()
            .IgnoreNullValues(true)
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Images)
            .Ignore(dest => dest.Properties);
    }
}