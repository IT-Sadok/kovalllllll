namespace DroneBuilder.Application.Models.WarehouseModels;

public class WarehouseItemModel
{
    public Guid Id { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
}