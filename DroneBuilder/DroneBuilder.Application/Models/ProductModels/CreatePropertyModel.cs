using Mapster.Models;

namespace DroneBuilder.Application.Models.ProductModels;

public class CreatePropertyModel
{
    public string Name { get; set; }
    public ICollection<CreateValueModel> Values { get; set; } = [];
}