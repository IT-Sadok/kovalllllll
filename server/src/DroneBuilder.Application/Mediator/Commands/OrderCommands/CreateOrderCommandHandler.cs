using System.Text.Json;
using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Contexts;
using DroneBuilder.Application.Mappings;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.OrderModels;
using DroneBuilder.Application.Options;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Events.OrderEvents;
using FluentResults;
namespace DroneBuilder.Application.Mediator.Commands.OrderCommands;

public class CreateOrderCommandHandler(
    IOrderRepository orderRepository,
    ICartRepository cartRepository,
    IProductRepository productRepository,
    IWarehouseRepository warehouseRepository,
    IOutboxEventService outboxService,
    IUnitOfWork unitOfWork,
    MessageQueuesConfiguration queuesConfig,
    IUserContext userContext) : ICommandHandler<CreateOrderCommand, OrderModel>
{
    public async Task<Result<OrderModel>> ExecuteCommandAsync(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        await using ITransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        Cart? cart = await cartRepository.GetCartByUserIdForUpdateAsync(userContext.UserId, cancellationToken);

        if (cart is null || cart.CartItems.Count == 0)
        {
            return Result.Fail<OrderModel>(new BadRequestError("Cart is empty."));
        }

        var productIds = cart.CartItems.Select(ci => ci.ProductId).ToList();

        ICollection<WarehouseItem> warehouseItems = await warehouseRepository
            .GetAllWarehouseItemsByProductIdsAsync(productIds, cancellationToken);

        var stockedProductIds = warehouseItems.Select(wi => wi.ProductId).ToHashSet();

        foreach (CartItem item in cart.CartItems)
        {
            if (!stockedProductIds.Contains(item.ProductId))
            {
                return Result.Fail<OrderModel>(new NotFoundError($"Product {item.ProductId} not found in warehouse."));
            }
        }

        Dictionary<Guid, Product> products = (await productRepository.GetProductsByIdsAsync(productIds, cancellationToken))
            .ToDictionary(p => p.Id);

        CartItem? unavailableItem = cart.CartItems.FirstOrDefault(ci => !products.ContainsKey(ci.ProductId));
        if (unavailableItem is not null)
        {
            return Result.Fail<OrderModel>(new BadRequestError(
                $"Product {unavailableItem.ProductName} is no longer available. Remove it from the cart to continue."));
        }

        var orderItems = cart.CartItems.Select(ci =>
        {
            Product product = products[ci.ProductId];

            return new OrderItem
            {
                ProductId = ci.ProductId,
                ProductName = product.Name,
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
        await transaction.CommitAsync(cancellationToken);

        return Result.Ok(order.ToModel());
    }
}

public record CreateOrderCommand(ShippingDetailsModel ShippingDetails);
