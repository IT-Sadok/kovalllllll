namespace DroneBuilder.Application.Models.ProductModels;

public class PropertyValuesResponseModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public ICollection<ValueResponseModel> Values { get; set; } = [];
}