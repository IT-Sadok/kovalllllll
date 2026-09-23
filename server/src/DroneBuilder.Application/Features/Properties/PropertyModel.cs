using DroneBuilder.Application.Features.Values;
namespace DroneBuilder.Application.Features.Properties;

public class PropertyModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<ValueModel> Values { get; set; } = [];
}
