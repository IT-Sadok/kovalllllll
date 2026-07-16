using DroneBuilder.Application.Models.OrderModels;
using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Mappings;

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

    public static Order ToEntity(this CreateOrderModel model)
    {
        if (model == null)
        {
            return null!;
        }

        return new Order
        {
            UserId = model.UserId,
            Status = model.Status,
            OrderItems = model.OrderItems?.Select(i => i.ToEntity()).ToList() ?? new List<OrderItem>(),
            TotalPrice = model.TotalPrice,
            ShippingDetails = model.ShippingDetails,
            CreatedAt = model.CreatedAt
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
            ProductName = item.Product?.Name ?? item.ProductName,
            ProductImageUrl = (item.Product?.Images != null && item.Product.Images.Any())
                ? (item.Product.Images.FirstOrDefault(x => x.IsPrimary) ?? item.Product.Images.First()).Url
                : string.Empty
        };
    }

    public static OrderItem ToEntity(this CreateOrderItemModel model)
    {
        if (model == null)
        {
            return null!;
        }

        return new OrderItem
        {
            ProductId = model.ProductId,
            Quantity = model.Quantity,
            PriceAtPurchase = model.Price
        };
    }

    public static OrderItem ToEntity(this OrderItemModel model)
    {
        if (model == null)
        {
            return null!;
        }

        return new OrderItem
        {
            ProductId = model.ProductId,
            Quantity = model.Quantity,
            PriceAtPurchase = model.Price
        };
    }
}
