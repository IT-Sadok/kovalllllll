using System.Text.Json;
using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Contexts;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.OrderModels;
using DroneBuilder.Application.Options;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Events.OrderEvents;
using FluentResults;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Commands.OrderCommands;

public class CreateOrderCommandHandler(
    IOrderRepository orderRepository,
    ICartRepository cartRepository,
    IProductRepository productRepository,
    IWarehouseRepository warehouseRepository,
    IOutboxEventService outboxService,
    MessageQueuesConfiguration queuesConfig,
    IUserContext userContext,
    IMapper mapper) : ICommandHandler<CreateOrderCommand, OrderModel>
{
    public async Task<Result<OrderModel>> ExecuteCommandAsync(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        Cart? cart = await cartRepository.GetCartByUserIdAsync(userContext.UserId, cancellationToken);
        if (cart is null || cart.CartItems.Count == 0)
        {
            return Result.Fail<OrderModel>(new BadRequestError("Cart is empty."));
        }

        var productIds = cart.CartItems.Select(ci => ci.ProductId).ToList();

        ICollection<WarehouseItem>? warehouseItem = await warehouseRepository
            .GetAllWarehouseItemsByProductIdsAsync(productIds, cancellationToken);

        foreach (CartItem item in cart.CartItems)
        {
            if (warehouseItem is null)
            {
                return Result.Fail<OrderModel>(new NotFoundError($"Product {item.ProductId} not found in warehouse."));
            }
        }

        ICollection<Product> products = await productRepository.GetProductsByIdsAsync(productIds, cancellationToken);

        var orderItems = cart.CartItems.Select(ci =>
        {
            Product product = products.First(p => p.Id == ci.ProductId);

            return new OrderItem
            {
                ProductId = ci.ProductId,
                Quantity = ci.Quantity,
                PriceAtPurchase = product.Price
            };
        }).ToList();

        var order = new Order
        {
            UserId = userContext.UserId,
            ShippingDetails = JsonSerializer.Serialize(command.ShippingDetails, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }),
            OrderItems = orderItems,
            TotalPrice = orderItems.Sum(i => i.PriceAtPurchase * i.Quantity)
        };

        await orderRepository.CreateOrderAsync(order, cancellationToken);
        await cartRepository.ClearCartAsync(cart.Id, cancellationToken);

        var @event = new OrderCreatedEvent(order.Id, userContext.UserId);
        await outboxService.StoreEventAsync(@event, queuesConfig.OrderQueue.Name, cancellationToken);

        await orderRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok(mapper.Map<OrderModel>(order));
    }
}

public record CreateOrderCommand(ShippingDetailsModel ShippingDetails);
