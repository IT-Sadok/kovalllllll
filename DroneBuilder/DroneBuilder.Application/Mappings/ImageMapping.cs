using DroneBuilder.Domain.Entities;
using DroneBuilder.Application.Models.ProductModels;
using Mapster;

namespace DroneBuilder.Application.Mappings;

public class ImageMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Image, ImageResponseModel>();
    }
}