using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Models.OrderModels;

public class CreateOrderModel
{
    public Guid UserId { get; set; }
    public string ShippingDetails { get; set; } = string.Empty;
    public Status Status { get; set; } = Status.New;
    public ICollection<OrderItemModel> OrderItems { get; set; } = [];
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}