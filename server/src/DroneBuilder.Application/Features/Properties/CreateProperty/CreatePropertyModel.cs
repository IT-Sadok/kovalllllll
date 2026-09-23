using DroneBuilder.Application.Features.Values;
namespace DroneBuilder.Application.Features.Properties.CreateProperty;

public class CreatePropertyModel
{
    public string Name { get; set; } = string.Empty;
    public ICollection<CreateValueModel> Values { get; set; } = [];
}
