using System.Text.Json;
using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Contexts;
using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.OrderModels;
using DroneBuilder.Application.Options;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Events.OrderEvents;
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
    public async Task<OrderModel> ExecuteCommandAsync(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var cart = await cartRepository.GetCartByUserIdAsync(userContext.UserId, cancellationToken);
        if (cart is null || cart.CartItems.Count == 0)
            throw new BadRequestException("Cart is empty.");

        var productIds = cart.CartItems.Select(ci => ci.ProductId).ToList();

        var warehouseItem = await warehouseRepository
            .GetAllWarehouseItemsByProductIdsAsync(productIds, cancellationToken);

        foreach (var item in cart.CartItems)
        {
            if (warehouseItem is null)
                throw new NotFoundException($"Product {item.ProductId} not found in warehouse.");
        }

        var products = await productRepository.GetProductsByIdsAsync(productIds, cancellationToken);

        var orderItems = cart.CartItems.Select(ci =>
        {
            var product = products.First(p => p.Id == ci.ProductId);

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
            ShippingDetails = JsonSerializer.Serialize(command.ShippingDetails),
            OrderItems = orderItems,
            TotalPrice = orderItems.Sum(i => i.PriceAtPurchase * i.Quantity)
        };

        await orderRepository.CreateOrderAsync(order, cancellationToken);
        await cartRepository.ClearCartAsync(cart.Id, cancellationToken);
        
        var @event = new OrderCreatedEvent(order.Id, userContext.UserId);
        await outboxService.StoreEventAsync(@event, queuesConfig.OrderQueue.Name, cancellationToken);
        
        await orderRepository.SaveChangesAsync(cancellationToken);

        return mapper.Map<OrderModel>(order);
    }
}

public record CreateOrderCommand(ShippingDetailsModel ShippingDetails);