namespace DroneBuilder.Domain.Entities;

public class Warehouse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public ICollection<WarehouseItem> WarehouseItems { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}