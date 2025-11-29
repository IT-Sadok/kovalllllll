using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Models.OrderModels;

public class OrderModel
{
    public Guid UserId { get; set; }
    public Status Status { get; set; }
    public ICollection<OrderItemModel> OrderItems { get; set; } = [];
    public decimal TotalPrice { get; set; }
    public string ShippingDetails { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}