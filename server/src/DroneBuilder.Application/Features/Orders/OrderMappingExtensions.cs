using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Features.Orders;

public static class OrderMappingExtensions
{
    public static OrderModel ToModel(this Order order)
    {
        if (order == null)
        {
            return null!;
        }

        return new OrderModel
        {
            Id = order.Id,
            UserId = order.UserId,
            UserEmail = order.User?.Email ?? string.Empty,
            Status = order.Status,
            OrderItems = order.OrderItems?.Select(i => i.ToModel()).ToList() ?? new List<OrderItemModel>(),
            TotalPrice = order.TotalPrice,
            ShippingDetails = order.ShippingDetails,
            CreatedAt = order.CreatedAt
        };
    }

    public static OrderItemModel ToModel(this OrderItem item)
    {
        if (item == null)
        {
            return null!;
        }

        return new OrderItemModel
        {
            ProductId = item.ProductId,
            Quantity = item.Quantity,
            Price = item.PriceAtPurchase,
            ProductName = string.IsNullOrEmpty(item.ProductName)
                ? item.Product?.Name ?? string.Empty
                : item.ProductName,
            ProductImageUrl = (item.Product?.Images != null && item.Product.Images.Any())
                ? (item.Product.Images.FirstOrDefault(x => x.IsPrimary) ?? item.Product.Images.First()).Url
                : string.Empty
        };
    }
}
