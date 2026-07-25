using DroneBuilder.Application.Common;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Mappings;

public static class PropertyMappingExtensions
{
    public static PropertyModel ToModel(this Property property)
    {
        if (property == null)
        {
            return null!;
        }

        return new PropertyModel
        {
            Id = property.Id,
            Name = property.Name,
            Values = property.Values?.Select(v => v.ToModel()).ToList() ?? new List<ValueModel>()
        };
    }

    public static Property ToEntity(this CreatePropertyModel model)
    {
        if (model == null)
        {
            return null!;
        }

        return new Property
        {
            Code = EntityCode.FromName(model.Name),
            Name = model.Name,
            Values = model.Values?.Select(v => v.ToEntity()).ToList() ?? new List<Value>()
        };
    }

    public static void UpdateEntity(this UpdatePropertyModel model, Property entity)
    {
        if (model == null || entity == null)
        {
            return;
        }

        if (model.Name != null)
        {
            entity.Name = model.Name;
        }
    }
}
