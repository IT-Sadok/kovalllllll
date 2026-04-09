namespace DroneBuilder.Domain.Entities;

public class ProductPropertyValue
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; }

    public Guid PropertyId { get; set; }
    public Property Property { get; set; }

    public Guid ValueId { get; set; }
    public Value Value { get; set; }
}
