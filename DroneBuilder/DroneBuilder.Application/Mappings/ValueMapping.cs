using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Domain.Entities;
using Mapster;

namespace DroneBuilder.Application.Mappings;

public class ValueMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Value, ValueResponseModel>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Text, src => src.Text);

        config.NewConfig<CreateValueModel, Value>()
            .Map(dest => dest.Text, src => src.Text)
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Properties);
    }
}