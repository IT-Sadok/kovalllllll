namespace DroneBuilder.Domain.Entities;

public class BuildItem
{
    public Guid Id { get; set; }
    public Guid BuildId { get; set; }
    public Build? Build { get; set; }
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; }
}
