namespace DroneBuilder.Application.Models.OrderModels;

public class CreateOrderItemModel
{
    public Guid ProductId { get; set; }
    public Guid OrderId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}