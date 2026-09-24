namespace DroneBuilder.Domain.Entities.Components;

public abstract class ComponentSpec
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public ComponentType Type { get; protected set; }
}
