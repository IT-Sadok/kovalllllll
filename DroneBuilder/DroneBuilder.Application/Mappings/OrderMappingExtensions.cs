using DroneBuilder.Application.Models.OrderModels;
using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Mappings;

public static class OrderMappingExtensions
{
    public static OrderModel ToModel(this Order order)
    {
        return new OrderModel
        {
            Id = order.Id,
            UserId = order.UserId,
            UserEmail = order.User?.Email ?? string.Empty,
            Status = order.Status,
            OrderItems = order.OrderItems.Select(item => item.ToModel()).ToList(),
            TotalPrice = order.TotalPrice,
            ShippingDetails = order.ShippingDetails,
            CreatedAt = order.CreatedAt
        };
    }

    public static OrderItemModel ToModel(this OrderItem item)
    {
        return new OrderItemModel
        {
            ProductId = item.ProductId,
            Quantity = item.Quantity,
            Price = item.PriceAtPurchase,
            ProductName = item.Product?.Name ?? item.ProductName,
            ProductImageUrl = item.Product?.Images.Count > 0
                ? (item.Product.Images.FirstOrDefault(image => image.IsPrimary) ?? item.Product.Images.First()).Url
                : string.Empty
        };
    }
}
