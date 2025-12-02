using DroneBuilder.Application.Models.OrderModels;
using DroneBuilder.Domain.Entities;
using Mapster;

namespace DroneBuilder.Application.Mappings;

public class OrderMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Order, OrderModel>()
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.Status, src => src.Status)
            .Map(dest => dest.OrderItems, src => src.OrderItems)
            .Map(dest => dest.TotalPrice, src => src.TotalPrice)
            .Map(dest => dest.ShippingDetails, src => src.ShippingDetails)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt);

        config.NewConfig<CreateOrderModel, Order>()
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.Status, src => src.Status)
            .Map(dest => dest.OrderItems, src => src.OrderItems)
            .Map(dest => dest.TotalPrice, src => src.TotalPrice)
            .Map(dest => dest.ShippingDetails, src => src.ShippingDetails)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt);
        config.NewConfig<OrderItem, OrderItemModel>()
            .Map(dest => dest.ProductId, src => src.ProductId)
            .Map(dest => dest.Quantity, src => src.Quantity)
            .Map(dest => dest.Price, src => src.PriceAtPurchase)
            .Map(dest => dest.ProductName, src =>
                src.Product != null ? src.Product.Name : src.ProductName);

        config.NewConfig<CreateOrderItemModel, OrderItem>()
            .Map(dest => dest.ProductId, src => src.ProductId)
            .Map(dest => dest.Quantity, src => src.Quantity)
            .Map(dest => dest.PriceAtPurchase, src => src.Price)
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.OrderId)
            .Ignore(dest => dest.Order)
            .Ignore(dest => dest.Product);
    }
}