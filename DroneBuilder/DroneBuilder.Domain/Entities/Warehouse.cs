namespace DroneBuilder.Domain.Entities;

public class Warehouse : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public ICollection<WarehouseItem> WarehouseItems { get; set; } = [];
}
