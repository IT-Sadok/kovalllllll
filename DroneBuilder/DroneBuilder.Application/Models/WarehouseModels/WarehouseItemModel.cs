namespace DroneBuilder.Application.Models.WarehouseModels;

public class WarehouseItemModel
{
    public Guid Id { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}