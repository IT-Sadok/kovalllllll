using Mapster;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Application.Models.ProductModels;

namespace DroneBuilder.Application.Mappings;

public class ProductMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Product, ProductModel>()
            .Map(dest => dest.Properties, src => src.ProductPropertyValues
                .GroupBy(ppv => ppv.Property.Id)
                .Select(g => new PropertyModel
                {
                    Id = g.Key,
                    Name = g.First().Property.Name,
                    Values = g.Select(ppv => new ValueModel
                    {
                        Id = ppv.Value.Id,
                        Text = ppv.Value.Text
                    }).ToList()
                }).ToList())
            .Map(dest => dest.Images, src => src.Images != null ? src.Images.OrderByDescending(i => i.IsPrimary).ToList() : null);


        config.NewConfig<Product, ProductPropertiesResponseModel>()
            .Map(dest => dest.Properties, src => src.ProductPropertyValues
                .GroupBy(ppv => ppv.Property.Id)
                .Select(g => new PropertyModel
                {
                    Id = g.Key,
                    Name = g.First().Property.Name,
                    Values = g.Select(ppv => new ValueModel
                    {
                        Id = ppv.Value.Id,
                        Text = ppv.Value.Text
                    }).ToList()
                }).ToList())
            .Map(dest => dest.Images, src => src.Images);


        config.NewConfig<CreateProductModel, Product>()
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Price, src => src.Price)
            .Map(dest => dest.Category, src => src.Category)
            .Ignore(dest => dest.ProductPropertyValues)
            .Ignore(dest => dest.Images)
            .Ignore(dest => dest.Id);

        config.NewConfig<UpdateProductRequestModel, Product>()
            .IgnoreNullValues(true)
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Images)
            .Ignore(dest => dest.ProductPropertyValues);
    }
}
