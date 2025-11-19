using Mapster;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Application.Models.ProductModels;

namespace DroneBuilder.Application.Mappings;

public class ProductMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Product, ProductModel>();

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