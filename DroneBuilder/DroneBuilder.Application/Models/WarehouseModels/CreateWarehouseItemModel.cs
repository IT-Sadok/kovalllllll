namespace DroneBuilder.Application.Models.WarehouseModels;

public class CreateWarehouseItemModel
{
    public Guid WarehouseId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}