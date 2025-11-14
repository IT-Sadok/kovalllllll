namespace DroneBuilder.Domain.Entities;

public class Value
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Text { get; set; }
    public ICollection<Property> Properties = new List<Property>();
}