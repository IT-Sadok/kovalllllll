namespace DroneBuilder.Application.Models.ProductModels;

public class CreatePropertyModel
{
    public string Name { get; set; } = string.Empty;
    public ICollection<CreateValueModel> Values { get; set; } = [];
}
