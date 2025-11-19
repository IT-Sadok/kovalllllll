namespace DroneBuilder.Application.Models.ProductModels;

public class PropertyModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public ICollection<ValueModel> Values { get; set; } = [];
}