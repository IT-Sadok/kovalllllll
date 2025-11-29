using System.Text.Json;
using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.OrderModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Commands.OrderCommands;

public class CreateOrderCommandHandler(
    IOrderRepository orderRepository,
    ICartRepository cartRepository,
    IProductRepository productRepository,
    IWarehouseRepository warehouseRepository,
    IMapper mapper) : ICommandHandler<CreateOrderCommand, OrderModel>
{
    public async Task<OrderModel> ExecuteCommandAsync(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var cart = await cartRepository.GetCartByUserIdAsync(command.UserId, cancellationToken);
        if (cart is null || cart.CartItems.Count == 0)
            throw new BadRequestException("Cart is empty.");

        foreach (var item in cart.CartItems)
        {
            var warehouseItem = await warehouseRepository
                .GetWarehouseItemByProductIdAsync(item.ProductId, cancellationToken);

            if (warehouseItem is null)
                throw new NotFoundException($"Product {item.ProductId} not found in warehouse.");
        }

        var productIds = cart.CartItems.Select(ci => ci.ProductId).ToList();
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
            UserId = command.UserId,
            ShippingDetails = JsonSerializer.Serialize(command.ShippingDetails),
            OrderItems = orderItems,
            TotalPrice = orderItems.Sum(i => i.PriceAtPurchase * i.Quantity)
        };

        await orderRepository.CreateOrderAsync(order, cancellationToken);
        await cartRepository.ClearCartAsync(cart.Id, cancellationToken);
        await orderRepository.SaveChangesAsync(cancellationToken);

        return mapper.Map<OrderModel>(order);
    }
}

public record CreateOrderCommand(Guid UserId, ShippingDetailsModel ShippingDetails);