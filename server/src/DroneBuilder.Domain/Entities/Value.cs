namespace DroneBuilder.Domain.Entities;

public class Value
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Text { get; set; } = string.Empty;
    public ICollection<Property> Properties { get; set; } = [];
}
