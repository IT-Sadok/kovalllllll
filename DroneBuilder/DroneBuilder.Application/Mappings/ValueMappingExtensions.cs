using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Mappings;

public static class ValueMappingExtensions
{
    public static ValueModel ToModel(this Value value)
    {
        if (value == null)
        {
            return null!;
        }

        return new ValueModel
        {
            Id = value.Id,
            Text = value.Text
        };
    }

    public static Value ToEntity(this CreateValueModel model)
    {
        if (model == null)
        {
            return null!;
        }

        return new Value
        {
            Text = model.Text
        };
    }

    public static void UpdateEntity(this UpdateValueModel model, Value entity)
    {
        if (model == null || entity == null)
        {
            return;
        }

        if (model.Text != null)
        {
            entity.Text = model.Text;
        }
    }
}
