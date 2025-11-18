using DroneBuilder.Domain.Entities;
using DroneBuilder.Application.Models.ProductModels;
using Mapster;

namespace DroneBuilder.Application.Mappings;

public class ImageMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Image, ImageResponseModel>();

        config.NewConfig<UploadImageModel, Image>()
            .Map(dest => dest.UploadedAt, src => DateTime.UtcNow)
            .Map(dest => dest.ProductId, src => src.ProductId)
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Product)
            .Ignore(dest => dest.Url)
            .Ignore(dest => dest.FileName);

        config.NewConfig<ICollection<Image>, ImagesResponseModel>()
            .Map(dest => dest.Images, src => src);

        config.NewConfig<Product, ProductImagesResponseModel>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Images, src => src.Images);
    }
}