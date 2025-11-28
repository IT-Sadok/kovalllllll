namespace DroneBuilder.Application.Models.WarehouseModels;

public class WarehouseModel
{
    public string Name { get; set; }
    public ICollection<WarehouseItemModel> WarehouseItems { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}