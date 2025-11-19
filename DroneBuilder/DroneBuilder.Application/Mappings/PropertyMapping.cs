using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Domain.Entities;
using Mapster;

namespace DroneBuilder.Application.Mappings;

public class PropertyMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Property, PropertyModel>()
            .Map(dest => dest.Values, src => src.Values);

        config.NewConfig<CreatePropertyModel, Property>()
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Values, src => src.Values)
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Products);

        config.NewConfig<UpdatePropertyModel, Property>()
            .IgnoreNullValues(true)
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Values)
            .Ignore(dest => dest.Products);


        config.NewConfig<Property, ICollection<PropertyModel>>();
    }
}